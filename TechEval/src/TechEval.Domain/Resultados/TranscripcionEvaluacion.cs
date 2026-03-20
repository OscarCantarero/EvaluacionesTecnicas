namespace TechEval.Domain.Resultados;

public enum TipoTranscripcion { Sesion = 1, Entrevista = 2 }
public enum EstadoTranscripcion { Pendiente = 1, Subida = 2, EvaluadaPorIA = 3 }

/// <summary>
/// Entidad que representa una transcripción de sesión o entrevista evaluable por IA.
/// Es hija del agregado ResultadoEvaluacion.
/// </summary>
public sealed class TranscripcionEvaluacion
{
    public Guid Id { get; private set; }
    public TipoTranscripcion Tipo { get; private set; }
    public string? Contenido { get; private set; }      // texto de transcripción
    public string? UrlArchivo { get; private set; }     // URL de archivo subido
    public decimal? PuntajeIA { get; private set; }     // 0-100
    public string? JustificacionIA { get; private set; }
    public EstadoTranscripcion Estado { get; private set; }
    public DateTime CreadaEn { get; private set; }

    private TranscripcionEvaluacion() { }

    public static TranscripcionEvaluacion Crear(
        TipoTranscripcion tipo,
        string? contenido = null,
        string? urlArchivo = null)
    {
        if (string.IsNullOrWhiteSpace(contenido) && string.IsNullOrWhiteSpace(urlArchivo))
            throw new ArgumentException("Se debe proporcionar al menos contenido de texto o URL de archivo.");

        return new TranscripcionEvaluacion
        {
            Id = Guid.NewGuid(),
            Tipo = tipo,
            Contenido = contenido,
            UrlArchivo = urlArchivo,
            Estado = EstadoTranscripcion.Subida,
            CreadaEn = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Registra el resultado de la evaluación por IA (puntaje 0-100 y justificación).
    /// </summary>
    public void RegistrarEvaluacionIA(decimal puntaje, string justificacion)
    {
        if (puntaje < 0 || puntaje > 100)
            throw new ArgumentOutOfRangeException(nameof(puntaje), "El puntaje debe estar entre 0 y 100");

        PuntajeIA = puntaje;
        JustificacionIA = justificacion;
        Estado = EstadoTranscripcion.EvaluadaPorIA;
    }
}
