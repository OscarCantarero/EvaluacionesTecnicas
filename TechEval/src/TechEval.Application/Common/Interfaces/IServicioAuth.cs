using TechEval.Application.Auth.Commands.Login;

namespace TechEval.Application.Common.Interfaces;

public interface IServicioAuth
{
    Task<TokenDto?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Auth.Commands.Refresh.TokenDto?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Guid> RegistrarUsuarioAsync(string email, string nombre, string password, string rol, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsuarioDto>> ListarUsuariosAsync(string? rol, CancellationToken cancellationToken = default);
}

public sealed record UsuarioDto(string Id, string Email, string Nombre, string Rol);
