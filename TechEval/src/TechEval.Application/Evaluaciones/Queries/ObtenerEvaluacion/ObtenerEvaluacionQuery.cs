using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Evaluaciones.Queries.ObtenerEvaluacion;

public sealed record ObtenerEvaluacionQuery(Guid Id) : IRequest<EvaluacionDto>;

public sealed record EvaluacionDto(
    Guid Id,
    string Nombre,
    string? Descripcion,
    string Estado,
    bool OrdenAleatorio,
    bool OrdenPorDificultad,
    string CreadoPor,
    DateTime CreadoEn,
    DateTime? ActualizadoEn,
    List<PreguntaDto> Preguntas);

public sealed record PreguntaDto(
    Guid Id,
    string Texto,
    string TipoPregunta,
    string NivelDificultad,
    int? LimiteTiempoSegundos,
    bool PermiteAdjunto,
    bool EsRevisionManual,
    int Orden,
    List<OpcionRespuestaDto> Opciones);

public sealed record OpcionRespuestaDto(
    Guid Id,
    string Texto,
    int? Puntuacion,
    bool EsRevisionManual,
    int Orden);

public sealed class ObtenerEvaluacionQueryHandler(IRepositorioEvaluacion repositorio)
    : IRequestHandler<ObtenerEvaluacionQuery, EvaluacionDto>
{
    public async Task<EvaluacionDto> Handle(ObtenerEvaluacionQuery query, CancellationToken cancellationToken)
    {
        var evaluacion = await repositorio.ObtenerConPreguntasAsync(query.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Evaluaciones.Evaluacion), query.Id);

        return new EvaluacionDto(
            evaluacion.Id,
            evaluacion.Nombre,
            evaluacion.Descripcion,
            evaluacion.Estado.ToString(),
            evaluacion.OrdenAleatorio,
            evaluacion.OrdenPorDificultad,
            evaluacion.CreadoPor,
            evaluacion.CreadoEn,
            evaluacion.ActualizadoEn,
            evaluacion.Preguntas.Select(p => new PreguntaDto(
                p.Id,
                p.Texto,
                p.TipoPregunta.ToString(),
                p.NivelDificultad.ToString(),
                p.LimiteTiempoSegundos,
                p.PermiteAdjunto,
                p.EsRevisionManual,
                p.Orden,
                p.Opciones.Select(o => new OpcionRespuestaDto(
                    o.Id,
                    o.Texto,
                    o.Puntuacion,
                    o.EsRevisionManual,
                    o.Orden)).ToList()
            )).ToList());
    }
}
