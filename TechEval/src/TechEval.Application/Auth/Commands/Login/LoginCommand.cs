using MediatR;

namespace TechEval.Application.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<TokenDto>;

public sealed record TokenDto(
    string AccessToken,
    string RefreshToken,
    DateTime Expiracion,
    string UsuarioId,
    string Email,
    List<string> Roles);
