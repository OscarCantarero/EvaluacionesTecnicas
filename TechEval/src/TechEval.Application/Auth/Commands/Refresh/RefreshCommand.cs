using MediatR;

namespace TechEval.Application.Auth.Commands.Refresh;

public sealed record RefreshCommand(string RefreshToken) : IRequest<TokenDto>;

public sealed record TokenDto(
    string AccessToken,
    string RefreshToken,
    DateTime Expiracion);
