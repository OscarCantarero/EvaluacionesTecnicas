using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Evaluaciones.Commands.AgregarPregunta;

public sealed class AgregarPreguntaCommandHandler(IRepositorioEvaluacion repositorio)
    : IRequestHandler<AgregarPreguntaCommand, Guid>
{
    public async Task<Guid> Handle(AgregarPreguntaCommand command, CancellationToken cancellationToken)
    {
        var evaluacion = await repositorio.ObtenerConPreguntasAsync(command.EvaluacionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Evaluaciones.Evaluacion), command.EvaluacionId);

        var pregunta = evaluacion.AgregarPregunta(
            command.Texto,
            command.TipoPregunta,
            command.NivelDificultad,
            command.LimiteTiempoSegundos,
            command.PermiteAdjunto,
            command.EsRevisionManual);

        await repositorio.ActualizarAsync(evaluacion, cancellationToken);
        return pregunta.Id;
    }
}
