using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Application.Common.Interfaces;

namespace TechEval.Application.Auth.Commands.Refresh;

public sealed class RefreshCommandHandler(IServicioAuth servicioAuth)
    : IRequestHandler<RefreshCommand, TokenDto>
{
    public async Task<TokenDto> Handle(RefreshCommand command, CancellationToken cancellationToken)
    {
        var token = await servicioAuth.RefreshAsync(command.RefreshToken, cancellationToken)
            ?? throw new UnauthorizedException("Refresh token inválido o expirado.");
        return token;
    }
}
