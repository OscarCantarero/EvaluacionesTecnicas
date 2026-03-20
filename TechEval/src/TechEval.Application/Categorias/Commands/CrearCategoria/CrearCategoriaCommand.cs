using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Application.Common.Interfaces;
using TechEval.Domain.Evaluaciones;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Categorias.Commands.CrearCategoria;

public sealed record CrearCategoriaCommand(string Nombre, string? Descripcion) : IRequest<Guid>;

public sealed class CrearCategoriaCommandHandler(
    IRepositorioCategoria repositorio,
    IContextoUsuario contextoUsuario)
    : IRequestHandler<CrearCategoriaCommand, Guid>
{
    public async Task<Guid> Handle(CrearCategoriaCommand command, CancellationToken cancellationToken)
    {
        if (await repositorio.ExistePorNombreAsync(command.Nombre, null, cancellationToken))
            throw new ConflictException($"Ya existe una categoría con el nombre '{command.Nombre}'.");

        var categoria = Categoria.Crear(command.Nombre, command.Descripcion, contextoUsuario.UsuarioId);
        await repositorio.AgregarAsync(categoria, cancellationToken);
        return categoria.Id;
    }
}
