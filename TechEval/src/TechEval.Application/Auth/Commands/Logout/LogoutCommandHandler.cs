using MediatR;
using TechEval.Application.Common.Interfaces;

namespace TechEval.Application.Auth.Commands.Logout;

public sealed class LogoutCommandHandler(IServicioAuth servicioAuth)
    : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand command, CancellationToken cancellationToken) =>
        await servicioAuth.LogoutAsync(command.RefreshToken, cancellationToken);
}
