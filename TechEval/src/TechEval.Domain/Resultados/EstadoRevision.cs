namespace TechEval.Domain.Resultados;

/// <summary>
/// Estados posibles del proceso de revisión de una evaluación
/// </summary>
public enum EstadoRevision
{
    /// <summary>Respuestas pendientes de revisión manual</summary>
    PendienteRevision = 1,

    /// <summary>Actualmente en proceso de revisión</summary>
    EnRevision = 2,

    /// <summary>Revisión completada, resultado final disponible</summary>
    Completada = 3
}
