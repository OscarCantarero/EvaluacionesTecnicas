using TechEval.Domain.Common.Abstractions;

namespace TechEval.Domain.Sesiones;

/// <summary>
/// Estados posibles de una sesión de evaluación
/// </summary>
public enum EstadoSesion
{
    NoIniciada = 1,     // Creada pero no comenzada por el candidato
    EnProgreso = 2,     // Candidato está respondiendo preguntas
    Completada = 3,     // Candidato finalizó todas las preguntas
    Abandonada = 4      // Candidato abandonó la sesión
}
