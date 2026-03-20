using TechEval.Domain.Common.Abstractions;

namespace TechEval.Domain.Resultados;

/// <summary>
/// Resultado general de una evaluación para un candidato
/// Agregado raíz que contiene todas las puntuaciones individuales
/// </summary>
public sealed class ResultadoEvaluacion : AgregadoRaiz
{
    /// <summary>ID del resultado</summary>
    public Guid Id { get; private set; }

    /// <summary>ID de la sesión a la que pertenece este resultado</summary>
    public Guid SesionId { get; private set; }

    /// <summary>ID de la evaluación</summary>
    public Guid EvaluacionId { get; private set; }

    /// <summary>ID del candidato</summary>
    public string CandidatoId { get; private set; } = null!;

    /// <summary>Nombre del candidato</summary>
    public string NombreCandidato { get; private set; } = null!;

    /// <summary>Título de la evaluación</summary>
    public string TituloEvaluacion { get; private set; } = null!;

    /// <summary>Fecha en que se completó la evaluación</summary>
    public DateTime FechaCompletacion { get; private set; }

    /// <summary>Puntuación total obtenida</summary>
    public decimal PuntuacionTotal { get; private set; }

    /// <summary>Puntuación máxima posible</summary>
    public decimal PuntuacionMaxima { get; private set; }

    /// <summary>Porcentaje obtenido (0-100)</summary>
    public decimal PorcentajeObtenido { get; private set; }

    /// <summary>Estado del proceso de revisión</summary>
    public EstadoRevision EstadoRevision { get; private set; }

    /// <summary>Fecha de inicio de la revisión</summary>
    public DateTime? FechaInicioRevision { get; private set; }

    /// <summary>Fecha de finalización de la revisión</summary>
    public DateTime? FechaFinRevision { get; private set; }

    /// <summary>Evaluador responsable de la revisión</summary>
    public string? EvaluadorRevision { get; private set; }

    /// <summary>Número de violaciones de pestaña detectadas</summary>
    public int ViolacionesPestana { get; private set; }

    /// <summary>Tiempo total empleado en la evaluación (segundos)</summary>
    public int TiempoTotalSegundos { get; private set; }

    /// <summary>Umbral de aprobación (porcentaje mínimo para aprobar)</summary>
    public const decimal UmbralAprobacion = 70m;

    /// <summary>Puntuaciones por pregunta</summary>
    private readonly List<PuntuacionPregunta> _puntuaciones = new();

    public IReadOnlyList<PuntuacionPregunta> Puntuaciones => _puntuaciones.AsReadOnly();

    /// <summary>Transcripciones de sesión y entrevista</summary>
    private readonly List<TranscripcionEvaluacion> _transcripciones = new();

    public IReadOnlyList<TranscripcionEvaluacion> Transcripciones => _transcripciones.AsReadOnly();

    private ResultadoEvaluacion() { }

    /// <summary>
    /// Crea un nuevo resultado de evaluación
    /// </summary>
    public static ResultadoEvaluacion Crear(
        Guid sesionId,
        Guid evaluacionId,
        string candidatoId,
        string nombreCandidato,
        string tituloEvaluacion,
        int violacionesPestana,
        int tiempoTotalSegundos)
    {
        return new ResultadoEvaluacion
        {
            Id = Guid.NewGuid(),
            SesionId = sesionId,
            EvaluacionId = evaluacionId,
            CandidatoId = candidatoId,
            NombreCandidato = nombreCandidato,
            TituloEvaluacion = tituloEvaluacion,
            FechaCompletacion = DateTime.UtcNow,
            EstadoRevision = EstadoRevision.PendienteRevision,
            ViolacionesPestana = violacionesPestana,
            TiempoTotalSegundos = tiempoTotalSegundos
        };
    }

    /// <summary>
    /// Agrega una transcripción de sesión o entrevista
    /// </summary>
    public void AgregarTranscripcion(TranscripcionEvaluacion transcripcion)
    {
        _transcripciones.Add(transcripcion);
    }

    /// <summary>
    /// Agrega una puntuación de pregunta
    /// </summary>
    public void AgregarPuntuacion(PuntuacionPregunta puntuacion)
    {
        if (_puntuaciones.Any(p => p.PreguntaSesionId == puntuacion.PreguntaSesionId))
            throw new InvalidOperationException("La pregunta ya tiene puntuación asignada");

        _puntuaciones.Add(puntuacion);
    }

    /// <summary>
    /// Calcula la puntuación total basada en las puntuaciones de preguntas y transcripciones con IA.
    /// Cuando hay transcripciones evaluadas por IA, el porcentaje final es el promedio de todos los componentes.
    /// <paramref name="puntuacionMaximaEvaluacion"/> es la suma real de puntos posibles de la evaluación.
    /// </summary>
    public void CalcularPuntuacionTotal(decimal puntuacionMaximaEvaluacion = 0m)
    {
        if (!_puntuaciones.Any())
        {
            PuntuacionTotal = 0m;
            PuntuacionMaxima = 0m;
            PorcentajeObtenido = 0m;
            return;
        }

        // Sumar puntuaciones finales de todas las preguntas
        PuntuacionTotal = _puntuaciones.Sum(p => p.ObtenerPuntuacionFinal());

        // Usar la puntuación máxima real de la evaluación
        PuntuacionMaxima = puntuacionMaximaEvaluacion > 0
            ? puntuacionMaximaEvaluacion
            : PuntuacionTotal; // Fallback: si no se proporcionó, usar total como máximo

        // Calcular porcentaje base de respuestas
        var porcentajeRespuestas = PuntuacionMaxima > 0
            ? Math.Round((PuntuacionTotal / PuntuacionMaxima) * 100m, 2)
            : 0m;

        // Incorporar transcripciones evaluadas por IA al promedio combinado
        var transcripcionesConIA = _transcripciones.Where(t => t.PuntajeIA.HasValue).ToList();
        if (transcripcionesConIA.Count > 0)
        {
            var componentesPuntaje = new List<decimal> { porcentajeRespuestas };
            foreach (var t in transcripcionesConIA)
                componentesPuntaje.Add(t.PuntajeIA!.Value);
            PorcentajeObtenido = Math.Round(componentesPuntaje.Average(), 2);
        }
        else
        {
            PorcentajeObtenido = porcentajeRespuestas;
        }
    }

    /// <summary>
    /// Inicia el proceso de revisión
    /// </summary>
    public void IniciarRevision(string evaluadorId)
    {
        EstadoRevision = EstadoRevision.EnRevision;
        FechaInicioRevision = DateTime.UtcNow;
        EvaluadorRevision = evaluadorId;
    }

    /// <summary>
    /// Completa el proceso de revisión
    /// </summary>
    public void CompletarRevision()
    {
        EstadoRevision = EstadoRevision.Completada;
        FechaFinRevision = DateTime.UtcNow;
    }

    /// <summary>
    /// Obtiene el estado general de la evaluación basado en el umbral de aprobación (70%).
    /// </summary>
    public string ObtenerEstadoGeneral()
    {
        return PorcentajeObtenido >= UmbralAprobacion ? "Aprobado" : "No aprobado";
    }

    /// <summary>
    /// Obtiene puntuación por pregunta
    /// </summary>
    public PuntuacionPregunta? ObtenerPuntuacion(Guid preguntaSesionId)
    {
        return _puntuaciones.FirstOrDefault(p => p.PreguntaSesionId == preguntaSesionId);
    }
}
