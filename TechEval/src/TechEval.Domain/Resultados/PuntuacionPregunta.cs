namespace TechEval.Domain.Resultados;

/// <summary>
/// Puntuación asignada a una pregunta de una sesión
/// </summary>
public sealed class PuntuacionPregunta
{
    /// <summary>ID único de esta puntuación</summary>
    public Guid Id { get; private set; }

    /// <summary>ID de la pregunta en la sesión</summary>
    public Guid PreguntaSesionId { get; private set; }

    /// <summary>Número de pregunta (1, 2, 3, ...)</summary>
    public int NumeroPregunta { get; private set; }

    /// <summary>Tipo de pregunta (TextoLibre, SeleccionUnica, SeleccionMultiple)</summary>
    public string TipoPregunta { get; private set; } = null!;

    /// <summary>Texto o respuesta del candidato</summary>
    public string Respuesta { get; private set; } = null!;

    /// <summary>Puntuación automática (si aplica)</summary>
    public decimal? PuntuacionAutomatica { get; private set; }

    /// <summary>Puntuación asignada manualmente por el evaluador</summary>
    public decimal? PuntuacionManual { get; private set; }

    /// <summary>Puntuación sugerida por IA (si aplica)</summary>
    public decimal? PuntuacionIASugerida { get; private set; }

    /// <summary>Justificación de la puntuación por IA</summary>
    public string? JustificacionIA { get; private set; }

    /// <summary>Observaciones del evaluador</summary>
    public string? Observaciones { get; private set; }

    /// <summary>¿La respuesta fue expirada?</summary>
    public bool FueExpirada { get; private set; }

    /// <summary>URL del adjunto si existe</summary>
    public string? UrlAdjunto { get; private set; }

    /// <summary>Tiempo empleado en segundos</summary>
    public int TiempoEmpleadoSegundos { get; private set; }

    private PuntuacionPregunta() { }

    public static PuntuacionPregunta Crear(
        Guid preguntaSesionId,
        int numeroPregunta,
        string tipoPregunta,
        string respuesta,
        decimal? puntuacionAutomatica = null,
        bool fueExpirada = false,
        string? urlAdjunto = null,
        int tiempoEmpleadoSegundos = 0)
    {
        return new PuntuacionPregunta
        {
            Id = Guid.NewGuid(),
            PreguntaSesionId = preguntaSesionId,
            NumeroPregunta = numeroPregunta,
            TipoPregunta = tipoPregunta,
            Respuesta = respuesta,
            PuntuacionAutomatica = puntuacionAutomatica,
            FueExpirada = fueExpirada,
            UrlAdjunto = urlAdjunto,
            TiempoEmpleadoSegundos = tiempoEmpleadoSegundos
        };
    }

    /// <summary>
    /// Asigna puntuación manual del evaluador
    /// </summary>
    public void AsignarPuntuacionManual(decimal puntaje, string? observaciones = null)
    {
        PuntuacionManual = puntaje;
        Observaciones = observaciones;
    }

    /// <summary>
    /// Registra sugerencia de puntuación de IA
    /// </summary>
    public void RegistrarSugerenciaIA(decimal puntajeSugerido, string justificacion)
    {
        PuntuacionIASugerida = puntajeSugerido;
        JustificacionIA = justificacion;
    }

    /// <summary>
    /// Obtiene la puntuación final (manual > automática > sugerencia IA)
    /// </summary>
    public decimal ObtenerPuntuacionFinal()
    {
        if (PuntuacionManual.HasValue)
            return PuntuacionManual.Value;

        if (PuntuacionAutomatica.HasValue)
            return PuntuacionAutomatica.Value;

        if (PuntuacionIASugerida.HasValue)
            return PuntuacionIASugerida.Value;

        return 0m;
    }
}
