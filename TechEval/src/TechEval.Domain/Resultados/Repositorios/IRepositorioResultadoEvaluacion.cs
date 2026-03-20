using TechEval.Domain.Common.Abstractions;

namespace TechEval.Domain.Resultados.Repositorios;

public interface IRepositorioResultadoEvaluacion : IRepositorio<ResultadoEvaluacion>
{
    /// <summary>
    /// Obtiene resultado por ID de sesión
    /// </summary>
    Task<ResultadoEvaluacion?> ObtenerPorSesionAsync(Guid sesionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los resultados de un candidato
    /// </summary>
    Task<List<ResultadoEvaluacion>> ObtenerPorCandidatoAsync(string candidatoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los resultados de una evaluación
    /// </summary>
    Task<List<ResultadoEvaluacion>> ObtenerPorEvaluacionAsync(Guid evaluacionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los resultados del sistema (para evaluadores/administradores)
    /// </summary>
    Task<List<ResultadoEvaluacion>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
}
