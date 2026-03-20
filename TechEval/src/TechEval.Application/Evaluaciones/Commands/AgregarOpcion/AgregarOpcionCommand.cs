using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Evaluaciones.Commands.AgregarOpcion;

public sealed record AgregarOpcionCommand(
    Guid EvaluacionId,
    Guid PreguntaId,
    string Texto,
    int? Puntuacion,
    bool EsRevisionManual
) : IRequest<Guid>;

public sealed class AgregarOpcionCommandHandler(IRepositorioEvaluacion repositorio)
    : IRequestHandler<AgregarOpcionCommand, Guid>
{
    public async Task<Guid> Handle(AgregarOpcionCommand command, CancellationToken cancellationToken)
    {
        var evaluacion = await repositorio.ObtenerConPreguntasAsync(command.EvaluacionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Evaluaciones.Evaluacion), command.EvaluacionId);

        var opcion = evaluacion.AgregarOpcion(command.PreguntaId, command.Texto, command.Puntuacion, command.EsRevisionManual);
        await repositorio.ActualizarAsync(evaluacion, cancellationToken);
        return opcion.Id;
    }
}
