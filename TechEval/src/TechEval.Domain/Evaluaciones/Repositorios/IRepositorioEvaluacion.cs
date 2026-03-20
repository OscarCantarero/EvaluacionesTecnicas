using TechEval.Domain.Common.Abstractions;

namespace TechEval.Domain.Evaluaciones.Repositorios;

public interface IRepositorioEvaluacion : IRepositorio<Evaluacion>
{
    Task<IReadOnlyList<Evaluacion>> ListarAsync(string? evaluadorId, CancellationToken cancellationToken = default);
    Task<Evaluacion?> ObtenerConPreguntasAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistePorNombreAsync(string nombre, Guid? excluirId, CancellationToken cancellationToken = default);
    Task<List<Pregunta>> ObtenerPreguntasBancoAsync(
        List<Guid>? categoriaIds,
        List<string>? dificultades,
        string? tipoPregunta,
        Guid? excluirEvaluacionId,
        CancellationToken cancellationToken = default);
}
