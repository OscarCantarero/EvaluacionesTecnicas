using TechEval.Domain.Common.Abstractions;
using TechEval.Domain.Sesiones.Events;

namespace TechEval.Domain.Sesiones;

/// <summary>
/// Agregado SesionEvaluacion - Representa la instancia de un candidato respondiendo una evaluación
/// </summary>
public sealed class SesionEvaluacion : AgregadoRaiz
{
    private readonly List<PreguntaSesion> _preguntas = [];

    public Guid Id { get; private set; }
    public Guid EvaluacionId { get; private set; }
    public string CandidatoId { get; private set; } = string.Empty;
    public string? CodigoAcceso { get; private set; }  // Link único para acceder a la sesión
    public EstadoSesion Estado { get; private set; }
    public DateTime CreadaEn { get; private set; }
    public DateTime? IniciadaEn { get; private set; }
    public DateTime? CompletadaEn { get; private set; }
    public int ContadorViolacionesPestana { get; private set; }
    public string? UrlGrabacionSesion { get; private set; }
    public string? UrlGrabacionAudio { get; private set; }
    public int MaxViolacionesPestana { get; private set; }  // 0 = sin límite
    public IReadOnlyCollection<PreguntaSesion> Preguntas => _preguntas.AsReadOnly();

    private SesionEvaluacion() { }

    /// <summary>
    /// Factory method para crear una nueva sesión
    /// </summary>
    public static SesionEvaluacion Crear(
        Guid evaluacionId,
        string candidatoId,
        List<(Guid PreguntaId, string Texto, int Orden, int? LimiteTiempo)> preguntas)
    {
        var sesion = new SesionEvaluacion
        {
            Id = Guid.NewGuid(),
            EvaluacionId = evaluacionId,
            CandidatoId = candidatoId,
            CodigoAcceso = Guid.NewGuid().ToString("N")[..12],  // ID corto para compartir
            Estado = EstadoSesion.NoIniciada,
            CreadaEn = DateTime.UtcNow
        };

        // Cargar preguntas en orden
        foreach (var (preguntaId, texto, orden, limiteTiempo) in preguntas.OrderBy(p => p.Orden))
        {
            var preguntaSesion = PreguntaSesion.Crear(evaluacionId, preguntaId, texto, orden, limiteTiempo);
            sesion._preguntas.Add(preguntaSesion);
        }

        return sesion;
    }

    /// <summary>
    /// El candidato inicia la sesión
    /// </summary>
    public void Iniciar()
    {
        if (Estado != EstadoSesion.NoIniciada)
            throw new InvalidOperationException("La sesión ya fue iniciada");

        Estado = EstadoSesion.EnProgreso;
        IniciadaEn = DateTime.UtcNow;
        RegistrarEvento(new SesionIniciadaEvent(Id, EvaluacionId, CandidatoId, IniciadaEn.Value));
    }

    /// <summary>
    /// Registra una respuesta del candidato
    /// </summary>
    public void RegistrarRespuesta(
        Guid preguntaId,
        string textoRespuesta,
        int tiempoEmpleadoSegundos,
        bool fueExpirado = false)
    {
        if (Estado != EstadoSesion.EnProgreso)
            throw new InvalidOperationException("Solo se pueden registrar respuestas en sesión en progreso");

        var pregunta = _preguntas.FirstOrDefault(p => p.PreguntaId == preguntaId)
            ?? throw new InvalidOperationException("Pregunta no encontrada en la sesión");

        if (pregunta.FueRespondida)
            throw new InvalidOperationException("La pregunta ya fue respondida");

        var respuesta = RespuestaCandidato.Crear(preguntaId, textoRespuesta, tiempoEmpleadoSegundos, fueExpirado);
        pregunta.RegistrarRespuesta(respuesta);

        RegistrarEvento(new RespuestaRegistradaEvent(
            Id,
            preguntaId,
            textoRespuesta,
            tiempoEmpleadoSegundos,
            respuesta.BrindadaEn
        ));
    }

    /// <summary>
    /// Adjunta un archivo a una respuesta
    /// </summary>
    public void AgregarAdjuntoARespuesta(Guid preguntaId, string urlAdjunto)
    {
        var pregunta = _preguntas.FirstOrDefault(p => p.PreguntaId == preguntaId)
            ?? throw new InvalidOperationException("Pregunta no encontrada en la sesión");

        if (pregunta.Respuesta == null)
            throw new InvalidOperationException("La pregunta no ha sido respondida");

        pregunta.Respuesta.AgregarAdjunto(urlAdjunto);
    }

    /// <summary>
    /// Registra una violación de pestaña (candidato cambió de ventana).
    /// Si se supera el límite máximo de violaciones, cancela la sesión automáticamente.
    /// </summary>
    public void RegistrarViolacionPestana()
    {
        ContadorViolacionesPestana++;
        RegistrarEvento(new ViolacionPestanaDetectadaEvent(
            Id,
            DateTime.UtcNow,
            ContadorViolacionesPestana
        ));

        if (MaxViolacionesPestana > 0 && ContadorViolacionesPestana >= MaxViolacionesPestana)
        {
            CancelarPorExcesoViolaciones();
        }
    }

    /// <summary>
    /// Cancela la sesión automáticamente por exceso de violaciones de pestaña.
    /// </summary>
    public void CancelarPorExcesoViolaciones()
    {
        if (Estado == EstadoSesion.Cancelada)
            return;

        Estado = EstadoSesion.Cancelada;
        CompletadaEn = DateTime.UtcNow;

        const int tiempoTotalCancelacion = 0;
        RegistrarEvento(new SesionCompletadaEvent(
            Id,
            CompletadaEn.Value,
            _preguntas.Count(p => p.FueRespondida),
            tiempoTotalCancelacion
        ));
    }

    /// <summary>
    /// Guarda la URL de la grabación de pantalla de la sesión.
    /// </summary>
    public void GuardarGrabacionSesion(string url)
    {
        UrlGrabacionSesion = url;
    }

    /// <summary>
    /// Guarda la URL de la grabación de audio de la sesión.
    /// </summary>
    public void GuardarGrabacionAudio(string url)
    {
        UrlGrabacionAudio = url;
    }

    /// <summary>
    /// Marca que el tiempo de una pregunta expiró
    /// </summary>
    public void MarcarTiempoExpirado(Guid preguntaId)
    {
        var pregunta = _preguntas.FirstOrDefault(p => p.PreguntaId == preguntaId)
            ?? throw new InvalidOperationException("Pregunta no encontrada");

        if (!pregunta.FueRespondida)
        {
            pregunta.MarcarTiempoExpirado();
            RegistrarEvento(new TiempoExpiratoEvent(Id, preguntaId, DateTime.UtcNow));
        }
    }

    /// <summary>
    /// Completa la sesión (todas las preguntas respondidas o candidato envía)
    /// </summary>
    public void Completar()
    {
        if (Estado != EstadoSesion.EnProgreso)
            throw new InvalidOperationException("Solo se pueden completar sesiones en progreso");

        Estado = EstadoSesion.Completada;
        CompletadaEn = DateTime.UtcNow;

        var preguntasRespondidas = _preguntas.Count(p => p.FueRespondida);
        var tiempoTotal = (int)(CompletadaEn.Value - IniciadaEn!.Value).TotalSeconds;

        RegistrarEvento(new SesionCompletadaEvent(
            Id,
            CompletadaEn.Value,
            preguntasRespondidas,
            tiempoTotal
        ));
    }

    /// <summary>
    /// Obtiene la pregunta actual (siguiente no respondida)
    /// </summary>
    public PreguntaSesion? ObtenerPreguntaActual()
    {
        return _preguntas.FirstOrDefault(p => !p.FueRespondida && !p.TiempoExpirado);
    }

    /// <summary>
    /// Obtiene el progreso de la sesión (preguntas respondidas / total)
    /// </summary>
    public (int Respondidas, int Total) ObtenerProgreso()
    {
        var respondidas = _preguntas.Count(p => p.FueRespondida);
        return (respondidas, _preguntas.Count);
    }

    /// <summary>
    /// Cargar preguntas desde EF Core (para consultas)
    /// </summary>
    internal void CargarPreguntas(IEnumerable<PreguntaSesion> preguntas)
    {
        _preguntas.Clear();
        _preguntas.AddRange(preguntas);
    }
}
