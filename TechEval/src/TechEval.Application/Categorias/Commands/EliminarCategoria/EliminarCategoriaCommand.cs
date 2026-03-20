using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Evaluaciones;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Categorias.Commands.EliminarCategoria;

public sealed record EliminarCategoriaCommand(Guid Id) : IRequest;

public sealed class EliminarCategoriaCommandHandler(IRepositorioCategoria repositorio)
    : IRequestHandler<EliminarCategoriaCommand>
{
    public async Task Handle(EliminarCategoriaCommand command, CancellationToken cancellationToken)
    {
        var categoria = await repositorio.ObtenerPorIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Categoria), command.Id);

        await repositorio.EliminarAsync(categoria, cancellationToken);
    }
}
