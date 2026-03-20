using TechEval.Domain.Common.Abstractions;

namespace TechEval.Domain.Sesiones.Repositorios;

public interface IRepositorioSesionEvaluacion : IRepositorio<SesionEvaluacion>
{
    /// <summary>
    /// Obtiene una sesión con sus preguntas y respuestas cargadas
    /// </summary>
    Task<SesionEvaluacion?> ObtenerConPreguntasAsync(Guid sesionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una sesión por su código de acceso (para candidatos)
    /// </summary>
    Task<SesionEvaluacion?> ObtenerPorCodigoAccesoAsync(string codigoAcceso, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las sesiones de un candidato
    /// </summary>
    Task<List<SesionEvaluacion>> ObtenerPorCandidatoAsync(string candidatoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las sesiones de una evaluación
    /// </summary>
    Task<List<SesionEvaluacion>> ObtenerPorEvaluacionAsync(Guid evaluacionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las sesiones del sistema (para evaluadores/administradores)
    /// </summary>
    Task<List<SesionEvaluacion>> ObtenerTodasAsync(CancellationToken cancellationToken = default);
}
