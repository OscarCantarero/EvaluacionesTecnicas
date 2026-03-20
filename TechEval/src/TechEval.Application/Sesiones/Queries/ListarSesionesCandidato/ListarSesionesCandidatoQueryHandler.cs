using MediatR;
using TechEval.Application.Common.DTOs;
using TechEval.Application.Common.Interfaces;
using TechEval.Domain.Evaluaciones.Repositorios;
using TechEval.Domain.Resultados.Repositorios;
using TechEval.Domain.Sesiones.Repositorios;

namespace TechEval.Application.Sesiones.Queries.ListarSesionesCandidato;

public sealed class ListarSesionesCandidatoQueryHandler : IRequestHandler<ListarSesionesCandidatoQuery, ResultadoPaginado<SesionResumenDto>>
{
    private readonly IRepositorioSesionEvaluacion _repo;
    private readonly IRepositorioEvaluacion _repoEvaluacion;
    private readonly IRepositorioResultadoEvaluacion _repoResultado;
    private readonly IContextoUsuario _contextoUsuario;

    public ListarSesionesCandidatoQueryHandler(
        IRepositorioSesionEvaluacion repo,
        IRepositorioEvaluacion repoEvaluacion,
        IRepositorioResultadoEvaluacion repoResultado,
        IContextoUsuario contextoUsuario)
    {
        _repo = repo;
        _repoEvaluacion = repoEvaluacion;
        _repoResultado = repoResultado;
        _contextoUsuario = contextoUsuario;
    }

    public async Task<ResultadoPaginado<SesionResumenDto>> Handle(
        ListarSesionesCandidatoQuery query,
        CancellationToken cancellationToken)
    {
        List<Domain.Sesiones.SesionEvaluacion> sesiones;
        List<Domain.Resultados.ResultadoEvaluacion> resultados;

        if (_contextoUsuario.EsEvaluador || _contextoUsuario.EsAdministrador)
        {
            sesiones = await _repo.ObtenerTodasAsync(cancellationToken);
            resultados = await _repoResultado.ObtenerTodosAsync(cancellationToken);
        }
        else
        {
            sesiones = await _repo.ObtenerPorCandidatoAsync(_contextoUsuario.UsuarioId, cancellationToken);
            resultados = await _repoResultado.ObtenerPorCandidatoAsync(_contextoUsuario.UsuarioId, cancellationToken);
        }

        var resultadosPorSesion = resultados.ToDictionary(r => r.SesionId);

        var evaluacionIds = sesiones.Select(s => s.EvaluacionId).Distinct();
        var titulosPorEvaluacion = new Dictionary<Guid, string>();
        foreach (var evalId in evaluacionIds)
        {
            var evaluacion = await _repoEvaluacion.ObtenerConPreguntasAsync(evalId, cancellationToken);
            titulosPorEvaluacion[evalId] = evaluacion?.Nombre ?? "Evaluación";
        }

        var mapped = sesiones.Select(s =>
        {
            resultadosPorSesion.TryGetValue(s.Id, out var resultado);
            titulosPorEvaluacion.TryGetValue(s.EvaluacionId, out var titulo);
            var (respondidas, total) = s.ObtenerProgreso();

            return new SesionResumenDto(
                SesionId: s.Id,
                EvaluacionId: s.EvaluacionId,
                EvaluacionTitulo: titulo ?? "Evaluación",
                Estado: s.Estado.ToString(),
                FechaCreacion: s.CreadaEn,
                FechaFin: s.CompletadaEn,
                PuntuacionObtenida: resultado?.PuntuacionTotal,
                ProgresoRespondidas: respondidas,
                ProgresoTotal: total
            );
        });

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(query.Estado))
            mapped = mapped.Where(s => s.Estado.Equals(query.Estado, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(query.Busqueda))
        {
            var termino = query.Busqueda.Trim();
            mapped = mapped.Where(s => s.EvaluacionTitulo.Contains(termino, StringComparison.OrdinalIgnoreCase));
        }

        var filtradas = mapped.ToList();
        var total_ = filtradas.Count;
        var items = filtradas
            .Skip((query.Pagina - 1) * query.TamanoPagina)
            .Take(query.TamanoPagina)
            .ToList();

        return new ResultadoPaginado<SesionResumenDto>(items, total_, query.Pagina, query.TamanoPagina);
    }
}
