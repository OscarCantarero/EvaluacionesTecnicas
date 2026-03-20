using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Evaluaciones;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Categorias.Commands.ActualizarCategoria;

public sealed record ActualizarCategoriaCommand(Guid Id, string Nombre, string? Descripcion) : IRequest;

public sealed class ActualizarCategoriaCommandHandler(IRepositorioCategoria repositorio)
    : IRequestHandler<ActualizarCategoriaCommand>
{
    public async Task Handle(ActualizarCategoriaCommand command, CancellationToken cancellationToken)
    {
        var categoria = await repositorio.ObtenerPorIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Categoria), command.Id);

        if (await repositorio.ExistePorNombreAsync(command.Nombre, command.Id, cancellationToken))
            throw new ConflictException($"Ya existe una categoría con el nombre '{command.Nombre}'.");

        categoria.Actualizar(command.Nombre, command.Descripcion);
        await repositorio.ActualizarAsync(categoria, cancellationToken);
    }
}
