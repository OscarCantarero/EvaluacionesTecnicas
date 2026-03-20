using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Evaluaciones.Commands.ActualizarEvaluacion;

public sealed class ActualizarEvaluacionCommandHandler(IRepositorioEvaluacion repositorio)
    : IRequestHandler<ActualizarEvaluacionCommand>
{
    public async Task Handle(ActualizarEvaluacionCommand command, CancellationToken cancellationToken)
    {
        var evaluacion = await repositorio.ObtenerPorIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Evaluaciones.Evaluacion), command.Id);

        evaluacion.Actualizar(command.Nombre, command.Descripcion);
        evaluacion.DesactivarOrden();

        if (command.OrdenAleatorio)
            evaluacion.ActivarOrdenAleatorio();
        else if (command.OrdenPorDificultad)
            evaluacion.ActivarOrdenPorDificultad();

        await repositorio.ActualizarAsync(evaluacion, cancellationToken);
    }
}
