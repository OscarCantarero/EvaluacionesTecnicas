using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Evaluaciones;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Evaluaciones.Commands.AgregarPreguntasMasivas;

public sealed record AgregarPreguntasMasivasCommand(
    Guid EvaluacionId,
    List<Guid> PreguntaIds
) : IRequest<AgregarPreguntasMasivasResponse>;

public sealed record AgregarPreguntasMasivasResponse(
    int PreguntasAgregadas,
    List<Guid> NuevosPreguntaIds
);

public sealed class AgregarPreguntasMasivasCommandHandler(IRepositorioEvaluacion repositorio)
    : IRequestHandler<AgregarPreguntasMasivasCommand, AgregarPreguntasMasivasResponse>
{
    public async Task<AgregarPreguntasMasivasResponse> Handle(
        AgregarPreguntasMasivasCommand command, CancellationToken cancellationToken)
    {
        var evaluacion = await repositorio.ObtenerConPreguntasAsync(command.EvaluacionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Evaluacion), command.EvaluacionId);

        var banco = await repositorio.ObtenerPreguntasBancoAsync(
            null, null, null, null, cancellationToken);

        var fuente = banco.Where(p => command.PreguntaIds.Contains(p.Id)).ToList();

        var nuevosIds = new List<Guid>();
        foreach (var preguntaFuente in fuente)
        {
            var nueva = evaluacion.AgregarPregunta(
                preguntaFuente.Texto,
                preguntaFuente.TipoPregunta,
                preguntaFuente.NivelDificultad,
                preguntaFuente.LimiteTiempoSegundos,
                preguntaFuente.PermiteAdjunto,
                preguntaFuente.EsRevisionManual,
                preguntaFuente.CategoriaId);

            foreach (var opcion in preguntaFuente.Opciones)
                evaluacion.AgregarOpcion(nueva.Id, opcion.Texto, opcion.Puntuacion, opcion.EsRevisionManual);

            nuevosIds.Add(nueva.Id);
        }

        await repositorio.ActualizarAsync(evaluacion, cancellationToken);

        return new AgregarPreguntasMasivasResponse(nuevosIds.Count, nuevosIds);
    }
}
