using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Common.ValueObjects;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Evaluaciones.Commands.ActivarEvaluacion;

public sealed class ActivarEvaluacionCommandHandler(IRepositorioEvaluacion repositorio)
    : IRequestHandler<ActivarEvaluacionCommand>
{
    public async Task Handle(ActivarEvaluacionCommand command, CancellationToken cancellationToken)
    {
        var evaluacion = await repositorio.ObtenerPorIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Evaluaciones.Evaluacion), command.Id);

        evaluacion.CambiarEstado(EstadoEvaluacion.Activa);

        await repositorio.ActualizarAsync(evaluacion, cancellationToken);
    }
}
