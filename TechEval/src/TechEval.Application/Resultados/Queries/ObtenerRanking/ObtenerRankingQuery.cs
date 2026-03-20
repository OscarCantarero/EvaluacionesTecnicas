using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Resultados;
using TechEval.Domain.Resultados.Repositorios;

namespace TechEval.Application.Resultados.Queries.ObtenerRanking;

public sealed record ObtenerRankingQuery(
    Guid EvaluacionId
) : IRequest<ObtenerRankingResponse>;

public sealed record ObtenerRankingResponse(
    Guid EvaluacionId,
    string TituloEvaluacion,
    int TotalCandidatos,
    List<CandidatoRankingDto> Ranking
);

public sealed record CandidatoRankingDto(
    int Posicion,
    Guid SesionId,
    string CandidatoId,
    string NombreCandidato,
    decimal PuntuacionTotal,
    decimal PorcentajeObtenido,
    string EstadoGeneral,
    int ViolacionesPestana,
    int TiempoTotalSegundos,
    string EstadoRevision,
    DateTime FechaCompletacion
);

public sealed class ObtenerRankingQueryHandler : IRequestHandler<ObtenerRankingQuery, ObtenerRankingResponse>
{
    private readonly IRepositorioResultadoEvaluacion _repositorio;

    public ObtenerRankingQueryHandler(IRepositorioResultadoEvaluacion repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<ObtenerRankingResponse> Handle(ObtenerRankingQuery request, CancellationToken cancellationToken)
    {
        var resultados = await _repositorio.ObtenerPorEvaluacionAsync(request.EvaluacionId, cancellationToken);

        if (resultados.Count == 0)
            throw new NotFoundException(nameof(ResultadoEvaluacion), request.EvaluacionId);

        var tituloEvaluacion = resultados.First().TituloEvaluacion;

        // Ordenar: mayor porcentaje primero, en empate menor tiempo gana
        var ordenados = resultados
            .OrderByDescending(r => r.PorcentajeObtenido)
            .ThenBy(r => r.TiempoTotalSegundos)
            .ThenBy(r => r.FechaCompletacion)
            .ToList();

        var ranking = ordenados.Select((r, index) => new CandidatoRankingDto(
            Posicion: index + 1,
            SesionId: r.SesionId,
            CandidatoId: r.CandidatoId,
            NombreCandidato: r.NombreCandidato,
            PuntuacionTotal: r.PuntuacionTotal,
            PorcentajeObtenido: r.PorcentajeObtenido,
            EstadoGeneral: r.ObtenerEstadoGeneral(),
            ViolacionesPestana: r.ViolacionesPestana,
            TiempoTotalSegundos: r.TiempoTotalSegundos,
            EstadoRevision: r.EstadoRevision.ToString(),
            FechaCompletacion: r.FechaCompletacion
        )).ToList();

        return new ObtenerRankingResponse(
            EvaluacionId: request.EvaluacionId,
            TituloEvaluacion: tituloEvaluacion,
            TotalCandidatos: ranking.Count,
            Ranking: ranking
        );
    }
}
