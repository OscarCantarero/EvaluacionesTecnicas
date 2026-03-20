using MediatR;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Evaluaciones.Queries.ObtenerPreguntasBanco;

public sealed record PreguntaBancoDto(
    Guid Id,
    Guid EvaluacionId,
    string TextoEvaluacion,
    string Texto,
    string TipoPregunta,
    string NivelDificultad,
    Guid? CategoriaId,
    string? NombreCategoria,
    int TotalOpciones
);

public sealed record ObtenerPreguntasBancoQuery(
    List<Guid>? CategoriaIds = null,
    List<string>? Dificultades = null,
    string? TipoPregunta = null,
    Guid? ExcluirEvaluacionId = null
) : IRequest<List<PreguntaBancoDto>>;

public sealed class ObtenerPreguntasBancoQueryHandler(
    IRepositorioEvaluacion repoEvaluacion,
    IRepositorioCategoria repoCategoria)
    : IRequestHandler<ObtenerPreguntasBancoQuery, List<PreguntaBancoDto>>
{
    public async Task<List<PreguntaBancoDto>> Handle(ObtenerPreguntasBancoQuery query, CancellationToken cancellationToken)
    {
        var preguntas = await repoEvaluacion.ObtenerPreguntasBancoAsync(
            query.CategoriaIds, query.Dificultades, query.TipoPregunta, query.ExcluirEvaluacionId, cancellationToken);

        var categorias = await repoCategoria.ListarAsync(cancellationToken);
        var catMap = categorias.ToDictionary(c => c.Id, c => c.Nombre);

        var evaluaciones = await repoEvaluacion.ListarAsync(null, cancellationToken);
        var evalMap = evaluaciones.ToDictionary(e => e.Id, e => e.Nombre);

        return preguntas.Select(p => new PreguntaBancoDto(
            Id: p.Id,
            EvaluacionId: p.EvaluacionId,
            TextoEvaluacion: evalMap.TryGetValue(p.EvaluacionId, out var n) ? n : "?",
            Texto: p.Texto,
            TipoPregunta: p.TipoPregunta.ToString(),
            NivelDificultad: p.NivelDificultad.ToString(),
            CategoriaId: p.CategoriaId,
            NombreCategoria: p.CategoriaId.HasValue && catMap.TryGetValue(p.CategoriaId.Value, out var cn) ? cn : null,
            TotalOpciones: p.Opciones.Count
        )).ToList();
    }
}
