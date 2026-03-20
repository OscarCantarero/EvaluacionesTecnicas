using MediatR;
using TechEval.Application.Common.Interfaces;

namespace TechEval.Application.Auth.Queries.ListarUsuarios;

public sealed record ListarUsuariosQuery(string? Rol = null) : IRequest<IReadOnlyList<UsuarioDto>>;

public sealed class ListarUsuariosQueryHandler(IServicioAuth servicioAuth)
    : IRequestHandler<ListarUsuariosQuery, IReadOnlyList<UsuarioDto>>
{
    public Task<IReadOnlyList<UsuarioDto>> Handle(ListarUsuariosQuery request, CancellationToken cancellationToken) =>
        servicioAuth.ListarUsuariosAsync(request.Rol, cancellationToken);
}
