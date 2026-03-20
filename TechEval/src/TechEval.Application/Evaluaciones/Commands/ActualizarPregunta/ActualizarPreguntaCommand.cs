using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Common.ValueObjects;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Evaluaciones.Commands.ActualizarPregunta;

public sealed record ActualizarPreguntaCommand(
    Guid EvaluacionId,
    Guid PreguntaId,
    string Texto,
    TipoPregunta TipoPregunta,
    NivelDificultad NivelDificultad,
    int? LimiteTiempoSegundos,
    bool PermiteAdjunto,
    bool EsRevisionManual,
    Guid? CategoriaId = null
) : IRequest;

public sealed class ActualizarPreguntaCommandHandler(IRepositorioEvaluacion repositorio)
    : IRequestHandler<ActualizarPreguntaCommand>
{
    public async Task Handle(ActualizarPreguntaCommand command, CancellationToken cancellationToken)
    {
        var evaluacion = await repositorio.ObtenerConPreguntasAsync(command.EvaluacionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Evaluaciones.Evaluacion), command.EvaluacionId);

        evaluacion.ActualizarPregunta(
            command.PreguntaId,
            command.Texto,
            command.TipoPregunta,
            command.NivelDificultad,
            command.LimiteTiempoSegundos,
            command.PermiteAdjunto,
            command.EsRevisionManual,
            command.CategoriaId);

        await repositorio.ActualizarAsync(evaluacion, cancellationToken);
    }
}
