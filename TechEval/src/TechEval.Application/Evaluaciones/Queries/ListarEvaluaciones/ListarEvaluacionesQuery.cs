using MediatR;
using TechEval.Application.Common.DTOs;
using TechEval.Application.Common.Interfaces;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Evaluaciones.Queries.ListarEvaluaciones;

public sealed record ListarEvaluacionesQuery(
    bool SoloMias = false,
    int Pagina = 1,
    int TamanoPagina = 10,
    string? Busqueda = null,
    string? Estado = null
) : IRequest<ResultadoPaginado<EvaluacionResumenDto>>;

public sealed record EvaluacionResumenDto(
    Guid Id,
    string Nombre,
    string? Descripcion,
    string Estado,
    bool OrdenAleatorio,
    bool OrdenPorDificultad,
    int TotalPreguntas,
    DateTime CreadoEn,
    string ModoSeleccionPreguntas);

public sealed class ListarEvaluacionesQueryHandler(
    IRepositorioEvaluacion repositorio,
    IContextoUsuario contextoUsuario)
    : IRequestHandler<ListarEvaluacionesQuery, ResultadoPaginado<EvaluacionResumenDto>>
{
    public async Task<ResultadoPaginado<EvaluacionResumenDto>> Handle(ListarEvaluacionesQuery query, CancellationToken cancellationToken)
    {
        var evaluadorId = query.SoloMias ? contextoUsuario.UsuarioId : null;
        var evaluaciones = await repositorio.ListarAsync(evaluadorId, cancellationToken);

        // Filtrar en memoria (repositorio ya carga todas)
        var filtradas = evaluaciones.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Busqueda))
        {
            var termino = query.Busqueda.Trim();
            filtradas = filtradas.Where(e =>
                e.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                (e.Descripcion?.Contains(termino, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (!string.IsNullOrWhiteSpace(query.Estado))
        {
            filtradas = filtradas.Where(e =>
                e.Estado.ToString().Equals(query.Estado, StringComparison.OrdinalIgnoreCase));
        }

        var total = filtradas.Count();
        var items = filtradas
            .Skip((query.Pagina - 1) * query.TamanoPagina)
            .Take(query.TamanoPagina)
            .Select(e => new EvaluacionResumenDto(
                e.Id, e.Nombre, e.Descripcion, e.Estado.ToString(),
                e.OrdenAleatorio, e.OrdenPorDificultad, e.Preguntas.Count, e.CreadoEn,
                e.ModoSeleccionPreguntas.ToString()))
            .ToList();

        return new ResultadoPaginado<EvaluacionResumenDto>(items, total, query.Pagina, query.TamanoPagina);
    }
}
