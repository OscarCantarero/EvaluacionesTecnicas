namespace TechEval.Domain.Evaluaciones;

/// <summary>Modo de selección de preguntas al crear una sesión.</summary>
public enum ModoSeleccionPreguntas
{
    /// <summary>Se usan todas las preguntas de la evaluación en orden configurado.</summary>
    Fijas = 0,
    /// <summary>Se seleccionan N preguntas aleatoriamente del pool de la evaluación.</summary>
    Aleatorias = 1,
    /// <summary>Se seleccionan preguntas respetando una distribución por dificultad.</summary>
    PorDistribucionDificultad = 2
}
