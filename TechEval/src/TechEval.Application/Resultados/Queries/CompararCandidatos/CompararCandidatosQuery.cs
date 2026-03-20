using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Resultados;
using TechEval.Domain.Resultados.Repositorios;

namespace TechEval.Application.Resultados.Queries.CompararCandidatos;

public sealed record CompararCandidatosQuery(
    Guid EvaluacionId,
    List<Guid> SesionIds
) : IRequest<CompararCandidatosResponse>;

public sealed record CompararCandidatosResponse(
    Guid EvaluacionId,
    string TituloEvaluacion,
    List<CandidatoComparativoDto> Candidatos,
    List<PreguntaComparativaDto> Preguntas
);

public sealed record CandidatoComparativoDto(
    Guid SesionId,
    string CandidatoId,
    string NombreCandidato,
    decimal PuntuacionTotal,
    decimal PorcentajeObtenido,
    string EstadoGeneral,
    int ViolacionesPestana,
    int TiempoTotalSegundos,
    string EstadoRevision
);

public sealed record PreguntaComparativaDto(
    int NumeroPregunta,
    string TipoPregunta,
    List<RespuestaComparativaDto> Respuestas
);

public sealed record RespuestaComparativaDto(
    Guid SesionId,
    string NombreCandidato,
    string Respuesta,
    decimal PuntuacionFinal,
    bool FueExpirada
);

public sealed class CompararCandidatosQueryHandler : IRequestHandler<CompararCandidatosQuery, CompararCandidatosResponse>
{
    private readonly IRepositorioResultadoEvaluacion _repositorio;

    public CompararCandidatosQueryHandler(IRepositorioResultadoEvaluacion repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<CompararCandidatosResponse> Handle(CompararCandidatosQuery request, CancellationToken cancellationToken)
    {
        if (request.SesionIds.Count < 2)
            throw new ValidationException([new ValidationError("SesionIds", "Se requieren al menos 2 sesiones para comparar.")]);

        // Obtener todos los resultados de la evaluación
        var todosResultados = await _repositorio.ObtenerPorEvaluacionAsync(request.EvaluacionId, cancellationToken);

        if (todosResultados.Count == 0)
            throw new NotFoundException(nameof(ResultadoEvaluacion), request.EvaluacionId);

        // Filtrar solo los sesionIds solicitados
        var resultados = todosResultados
            .Where(r => request.SesionIds.Contains(r.SesionId))
            .ToList();

        if (resultados.Count < 2)
            throw new ValidationException([new ValidationError("SesionIds", "No se encontraron al menos 2 resultados completados para las sesiones indicadas.")]);

        var tituloEvaluacion = resultados.First().TituloEvaluacion;

        // Construir candidatos comparativos
        var candidatos = resultados
            .OrderByDescending(r => r.PorcentajeObtenido)
            .Select(r => new CandidatoComparativoDto(
                SesionId: r.SesionId,
                CandidatoId: r.CandidatoId,
                NombreCandidato: r.NombreCandidato,
                PuntuacionTotal: r.PuntuacionTotal,
                PorcentajeObtenido: r.PorcentajeObtenido,
                EstadoGeneral: r.ObtenerEstadoGeneral(),
                ViolacionesPestana: r.ViolacionesPestana,
                TiempoTotalSegundos: r.TiempoTotalSegundos,
                EstadoRevision: r.EstadoRevision.ToString()
            ))
            .ToList();

        // Construir comparativa por pregunta
        var todasPreguntas = resultados
            .SelectMany(r => r.Puntuaciones)
            .GroupBy(p => p.NumeroPregunta)
            .OrderBy(g => g.Key)
            .Select(g => new PreguntaComparativaDto(
                NumeroPregunta: g.Key,
                TipoPregunta: g.First().TipoPregunta,
                Respuestas: g.Select(p =>
                {
                    var resultado = resultados.First(r => r.Puntuaciones.Contains(p));
                    return new RespuestaComparativaDto(
                        SesionId: resultado.SesionId,
                        NombreCandidato: resultado.NombreCandidato,
                        Respuesta: p.Respuesta,
                        PuntuacionFinal: p.ObtenerPuntuacionFinal(),
                        FueExpirada: p.FueExpirada
                    );
                })
                .OrderByDescending(r => r.PuntuacionFinal)
                .ToList()
            ))
            .ToList();

        return new CompararCandidatosResponse(
            EvaluacionId: request.EvaluacionId,
            TituloEvaluacion: tituloEvaluacion,
            Candidatos: candidatos,
            Preguntas: todasPreguntas
        );
    }
}
