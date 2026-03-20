using MediatR;
using TechEval.Application.Common.Interfaces;

namespace TechEval.Application.Auth.Commands.RegistrarUsuario;

public sealed class RegistrarUsuarioCommandHandler(IServicioAuth servicioAuth)
    : IRequestHandler<RegistrarUsuarioCommand, Guid>
{
    public async Task<Guid> Handle(RegistrarUsuarioCommand command, CancellationToken cancellationToken) =>
        await servicioAuth.RegistrarUsuarioAsync(command.Email, command.Nombre, command.Password, command.Rol, cancellationToken);
}
