using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechEval.Application.Auth.Commands.Login;
using TechEval.Application.Auth.Commands.Logout;
using TechEval.Application.Auth.Commands.Refresh;
using TechEval.Application.Auth.Commands.RegistrarUsuario;
using TechEval.Application.Auth.Queries.ListarUsuarios;

namespace TechEval.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    /// <summary>Inicia sesión y obtiene tokens JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(command, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>Renueva el access token usando un refresh token válido.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshCommand command, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(command, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>Cierra la sesión revocando el refresh token.</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>Lista usuarios del sistema. Evaluador y Administrador. Filtrar por rol con ?rol=Candidato.</summary>
    [HttpGet("usuarios")]
    [Authorize(Roles = "Evaluador,Administrador")]
    public async Task<IActionResult> ListarUsuarios([FromQuery] string? rol, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new ListarUsuariosQuery(rol), cancellationToken);
        return Ok(resultado);
    }

    /// <summary>Registra un nuevo usuario. Solo Administrador.</summary>
    [HttpPost("registrar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Registrar), new { id }, new { Id = id });
    }
}
