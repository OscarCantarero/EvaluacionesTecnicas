using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Evaluaciones.Commands.EliminarEvaluacion;

public sealed record EliminarEvaluacionCommand(Guid Id) : IRequest;

public sealed class EliminarEvaluacionCommandHandler(IRepositorioEvaluacion repositorio)
    : IRequestHandler<EliminarEvaluacionCommand>
{
    public async Task Handle(EliminarEvaluacionCommand command, CancellationToken cancellationToken)
    {
        var evaluacion = await repositorio.ObtenerPorIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Evaluaciones.Evaluacion), command.Id);

        await repositorio.EliminarAsync(evaluacion, cancellationToken);
    }
}
