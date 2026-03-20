using MediatR;

namespace TechEval.Application.Auth.Commands.RegistrarUsuario;

public sealed record RegistrarUsuarioCommand(
    string Email,
    string Nombre,
    string Password,
    string Rol
) : IRequest<Guid>;
