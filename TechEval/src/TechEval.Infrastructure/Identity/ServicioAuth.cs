using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TechEval.Application.Auth.Commands.Login;
using TechEval.Application.Common.Interfaces;
using TechEval.Infrastructure.Persistence;

namespace TechEval.Infrastructure.Identity;

public sealed class ServicioAuth(
    UserManager<UsuarioApp> userManager,
    TechEvalDbContext context,
    IConfiguration configuration)
    : IServicioAuth
{
    public async Task<TokenDto?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var usuario = await userManager.FindByEmailAsync(email);
        if (usuario is null || !await userManager.CheckPasswordAsync(usuario, password))
            return null;

        var roles = await userManager.GetRolesAsync(usuario);
        var (accessToken, expiracion) = GenerarAccessToken(usuario, roles);
        var refreshToken = await GenerarRefreshTokenAsync(usuario.Id, cancellationToken);

        return new TokenDto(accessToken, refreshToken, expiracion, usuario.Id, usuario.Email!, [.. roles]);
    }

    public async Task<Application.Auth.Commands.Refresh.TokenDto?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var entrada = await context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken && !r.Revocado && r.Expiracion > DateTime.UtcNow, cancellationToken);

        if (entrada is null) return null;

        var usuario = await userManager.FindByIdAsync(entrada.UsuarioId);
        if (usuario is null) return null;

        // Rotar refresh token
        entrada.Revocado = true;
        var roles = await userManager.GetRolesAsync(usuario);
        var (accessToken, expiracion) = GenerarAccessToken(usuario, roles);
        var nuevoRefreshToken = await GenerarRefreshTokenAsync(usuario.Id, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return new Application.Auth.Commands.Refresh.TokenDto(accessToken, nuevoRefreshToken, expiracion);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var entrada = await context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken, cancellationToken);

        if (entrada is not null)
        {
            entrada.Revocado = true;
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<Application.Common.Interfaces.UsuarioDto>> ListarUsuariosAsync(string? rol, CancellationToken cancellationToken = default)
    {
        IList<UsuarioApp> usuarios;
        if (!string.IsNullOrWhiteSpace(rol))
        {
            usuarios = await userManager.GetUsersInRoleAsync(rol);
        }
        else
        {
            usuarios = await userManager.Users.ToListAsync(cancellationToken);
        }

        var resultado = new List<Application.Common.Interfaces.UsuarioDto>();
        foreach (var u in usuarios)
        {
            var roles = await userManager.GetRolesAsync(u);
            resultado.Add(new Application.Common.Interfaces.UsuarioDto(
                u.Id,
                u.Email ?? string.Empty,
                u.Nombre,
                roles.FirstOrDefault() ?? string.Empty));
        }

        return resultado.AsReadOnly();
    }

    public async Task<Guid> RegistrarUsuarioAsync(string email, string nombre, string password, string rol, CancellationToken cancellationToken = default)
    {
        var usuario = new UsuarioApp
        {
            UserName = email,
            Email = email,
            Nombre = nombre,
            EmailConfirmed = true
        };

        var resultado = await userManager.CreateAsync(usuario, password);
        if (!resultado.Succeeded)
            throw new InvalidOperationException(string.Join(", ", resultado.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(usuario, rol);
        return Guid.Parse(usuario.Id);
    }

    private (string token, DateTime expiracion) GenerarAccessToken(UsuarioApp usuario, IList<string> roles)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var expiracion = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpirationMinutes"]!));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id),
            new(JwtRegisteredClaimNames.Email, usuario.Email!),
            new(JwtRegisteredClaimNames.Name, usuario.Nombre),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiracion,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expiracion);
    }

    private async Task<string> GenerarRefreshTokenAsync(string usuarioId, CancellationToken cancellationToken)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var entrada = new RefreshTokenEntry
        {
            UsuarioId = usuarioId,
            Token = token,
            Expiracion = DateTime.UtcNow.AddDays(double.Parse(jwtSettings["RefreshExpirationDays"]!))
        };
        context.RefreshTokens.Add(entrada);
        await context.SaveChangesAsync(cancellationToken);
        return token;
    }
}
