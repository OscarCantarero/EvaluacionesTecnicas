namespace TechEval.Domain.Sesiones;

/// <summary>
/// Respuesta brindada por un candidato a una pregunta de la sesión
/// </summary>
public sealed class RespuestaCandidato
{
    public Guid Id { get; private set; }
    public Guid PreguntaId { get; private set; }
    public string Texto { get; private set; } = string.Empty;
    public string? UrlAdjunto { get; private set; }  // Ruta a archivo adjunto si aplica
    public int TiempoEmpleadoSegundos { get; private set; }
    public DateTime BrindadaEn { get; private set; }
    public bool FueExpiratoElTiempo { get; private set; }  // true si se envió después de expirar

    private RespuestaCandidato() { }

    public static RespuestaCandidato Crear(
        Guid preguntaId,
        string texto,
        int tiempoEmpleadoSegundos,
        bool fueExpirado = false)
    {
        return new RespuestaCandidato
        {
            Id = Guid.NewGuid(),
            PreguntaId = preguntaId,
            Texto = texto,
            TiempoEmpleadoSegundos = tiempoEmpleadoSegundos,
            FueExpiratoElTiempo = fueExpirado,
            BrindadaEn = DateTime.UtcNow
        };
    }

    public void AgregarAdjunto(string urlAdjunto)
    {
        UrlAdjunto = urlAdjunto;
    }
}

/// <summary>
/// Pregunta dentro de una sesión (referencia a pregunta de evaluación con estado de sesión)
/// </summary>
public sealed class PreguntaSesion
{
    public Guid Id { get; private set; }
    public Guid EvaluacionId { get; private set; }
    public Guid PreguntaId { get; private set; }  // FK a pregunta original
    public string Texto { get; private set; } = string.Empty;
    public int Orden { get; private set; }
    public int? LimiteTiempoSegundos { get; private set; }
    public RespuestaCandidato? Respuesta { get; private set; }
    public bool FueRespondida { get; private set; }
    public bool TiempoExpirado { get; private set; }

    private PreguntaSesion() { }

    public static PreguntaSesion Crear(
        Guid evaluacionId,
        Guid preguntaId,
        string texto,
        int orden,
        int? limiteTiempoSegundos)
    {
        return new PreguntaSesion
        {
            Id = Guid.NewGuid(),
            EvaluacionId = evaluacionId,
            PreguntaId = preguntaId,
            Texto = texto,
            Orden = orden,
            LimiteTiempoSegundos = limiteTiempoSegundos,
            FueRespondida = false,
            TiempoExpirado = false
        };
    }

    public void RegistrarRespuesta(RespuestaCandidato respuesta)
    {
        Respuesta = respuesta;
        FueRespondida = true;
    }

    public void MarcarTiempoExpirado()
    {
        TiempoExpirado = true;
    }
}
