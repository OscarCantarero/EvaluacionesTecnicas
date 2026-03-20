namespace TechEval.Domain.Evaluaciones;

/// <summary>Distribución de preguntas por nivel de dificultad para una sesión.</summary>
public sealed record DistribucionDificultad(int Facil, int Medio, int Dificil)
{
    public int Total => Facil + Medio + Dificil;

    public static DistribucionDificultad Crear(int facil, int medio, int dificil)
    {
        if (facil < 0 || medio < 0 || dificil < 0)
            throw new ArgumentException("Todos los valores de distribución deben ser ≥ 0.");
        if (facil + medio + dificil == 0)
            throw new ArgumentException("La distribución total debe ser mayor que 0.");
        return new DistribucionDificultad(facil, medio, dificil);
    }
}
