using MediatR;
using TechEval.Application.Common.DTOs;

namespace TechEval.Application.Sesiones.Queries.ListarSesionesCandidato;

public sealed record ListarSesionesCandidatoQuery(
    int Pagina = 1,
    int TamanoPagina = 10,
    string? Estado = null,
    string? Busqueda = null
) : IRequest<ResultadoPaginado<SesionResumenDto>>;

public sealed record SesionResumenDto(
    Guid SesionId,
    Guid EvaluacionId,
    string EvaluacionTitulo,
    string Estado,
    DateTime FechaCreacion,
    DateTime? FechaFin,
    decimal? PuntuacionObtenida,
    int ProgresoRespondidas,
    int ProgresoTotal
);
