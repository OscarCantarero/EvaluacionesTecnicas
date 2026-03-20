using TechEval.Domain.Common.Abstractions;
using TechEval.Domain.Common.ValueObjects;
using TechEval.Domain.Evaluaciones.Events;

namespace TechEval.Domain.Evaluaciones;

public sealed class Evaluacion : AgregadoRaiz
{
    private readonly List<Pregunta> _preguntas = [];

    public Guid Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public EstadoEvaluacion Estado { get; private set; }
    public bool OrdenAleatorio { get; private set; }
    public bool OrdenPorDificultad { get; private set; }
    public ModoSeleccionPreguntas ModoSeleccionPreguntas { get; private set; } = ModoSeleccionPreguntas.Fijas;
    public int? CantidadPreguntasSesion { get; private set; }
    public DistribucionDificultad? DistribucionDificultad { get; private set; }
    public string CreadoPor { get; private set; } = string.Empty;
    public DateTime CreadoEn { get; private set; }
    public DateTime? ActualizadoEn { get; private set; }
    public IReadOnlyCollection<Pregunta> Preguntas => _preguntas.AsReadOnly();

    private Evaluacion() { }

    public static Evaluacion Crear(string nombre, string? descripcion, string creadoPor)
    {
        var evaluacion = new Evaluacion
        {
            Id = Guid.NewGuid(),
            Nombre = nombre,
            Descripcion = descripcion,
            Estado = EstadoEvaluacion.Borrador,
            CreadoPor = creadoPor,
            CreadoEn = DateTime.UtcNow
        };

        evaluacion.RegistrarEvento(new EvaluacionCreadaEvent(evaluacion.Id, evaluacion.Nombre));
        return evaluacion;
    }

    public void Actualizar(string nombre, string? descripcion)
    {
        Nombre = nombre;
        Descripcion = descripcion;
        ActualizadoEn = DateTime.UtcNow;
    }

    public void ActivarOrdenAleatorio()
    {
        if (OrdenPorDificultad)
            throw ErroresEvaluacion.OrdenConflicto.ToException();
        OrdenAleatorio = true;
        ActualizadoEn = DateTime.UtcNow;
    }

    public void ActivarOrdenPorDificultad()
    {
        if (OrdenAleatorio)
            throw ErroresEvaluacion.OrdenConflicto.ToException();
        OrdenPorDificultad = true;
        ActualizadoEn = DateTime.UtcNow;
    }

    public void DesactivarOrden()
    {
        OrdenAleatorio = false;
        OrdenPorDificultad = false;
        ActualizadoEn = DateTime.UtcNow;
    }

    public void CambiarEstado(EstadoEvaluacion nuevoEstado)
    {
        Estado = nuevoEstado;
        ActualizadoEn = DateTime.UtcNow;
    }

    public Pregunta AgregarPregunta(
        string texto,
        Common.ValueObjects.TipoPregunta tipo,
        Common.ValueObjects.NivelDificultad nivel,
        int? limiteTiempoSegundos,
        bool permiteAdjunto,
        bool esRevisionManual,
        Guid? categoriaId = null)
    {
        var orden = _preguntas.Count + 1;
        var pregunta = Pregunta.Crear(Id, texto, tipo, nivel, limiteTiempoSegundos, permiteAdjunto, esRevisionManual, orden, categoriaId);
        _preguntas.Add(pregunta);
        ActualizadoEn = DateTime.UtcNow;
        return pregunta;
    }

    public void ActualizarPregunta(
        Guid preguntaId,
        string texto,
        Common.ValueObjects.TipoPregunta tipo,
        Common.ValueObjects.NivelDificultad nivel,
        int? limiteTiempoSegundos,
        bool permiteAdjunto,
        bool esRevisionManual,
        Guid? categoriaId = null)
    {
        var pregunta = _preguntas.FirstOrDefault(p => p.Id == preguntaId)
            ?? throw ErroresEvaluacion.PreguntaNoEncontrada.ToException();
        pregunta.Actualizar(texto, tipo, nivel, limiteTiempoSegundos, permiteAdjunto, esRevisionManual);
        pregunta.AsignarCategoria(categoriaId);
        ActualizadoEn = DateTime.UtcNow;
    }

    public void EliminarPregunta(Guid preguntaId)
    {
        var pregunta = _preguntas.FirstOrDefault(p => p.Id == preguntaId)
            ?? throw ErroresEvaluacion.PreguntaNoEncontrada.ToException();
        _preguntas.Remove(pregunta);
        ActualizadoEn = DateTime.UtcNow;
    }

    public OpcionRespuesta AgregarOpcion(Guid preguntaId, string texto, int? puntuacion, bool esRevisionManual)
    {
        var pregunta = _preguntas.FirstOrDefault(p => p.Id == preguntaId)
            ?? throw ErroresEvaluacion.PreguntaNoEncontrada.ToException();
        ActualizadoEn = DateTime.UtcNow;
        return pregunta.AgregarOpcion(texto, puntuacion, esRevisionManual);
    }

    public void ActualizarOpcion(Guid preguntaId, Guid opcionId, string texto, int? puntuacion, bool esRevisionManual)
    {
        var pregunta = _preguntas.FirstOrDefault(p => p.Id == preguntaId)
            ?? throw ErroresEvaluacion.PreguntaNoEncontrada.ToException();
        pregunta.ActualizarOpcion(opcionId, texto, puntuacion, esRevisionManual);
        ActualizadoEn = DateTime.UtcNow;
    }

    public void EliminarOpcion(Guid preguntaId, Guid opcionId)
    {
        var pregunta = _preguntas.FirstOrDefault(p => p.Id == preguntaId)
            ?? throw ErroresEvaluacion.PreguntaNoEncontrada.ToException();
        pregunta.EliminarOpcion(opcionId);
        ActualizadoEn = DateTime.UtcNow;
    }

    public void ConfigurarSeleccionDinamica(
        ModoSeleccionPreguntas modo,
        int? cantidadPreguntasSesion,
        DistribucionDificultad? distribucionDificultad)
    {
        if (modo == ModoSeleccionPreguntas.Aleatorias)
        {
            if (!cantidadPreguntasSesion.HasValue || cantidadPreguntasSesion <= 0)
                throw ErroresEvaluacion.CantidadPreguntasInvalida.ToException();
        }

        if (modo == ModoSeleccionPreguntas.PorDistribucionDificultad)
        {
            if (distribucionDificultad == null)
                throw ErroresEvaluacion.DistribucionRequerida.ToException();
        }

        ModoSeleccionPreguntas = modo;
        CantidadPreguntasSesion = modo == ModoSeleccionPreguntas.Fijas ? null : cantidadPreguntasSesion;
        DistribucionDificultad = modo == ModoSeleccionPreguntas.PorDistribucionDificultad ? distribucionDificultad : null;
        ActualizadoEn = DateTime.UtcNow;
    }

    // Needed for EF Core collection loading
    internal void CargarPreguntas(IEnumerable<Pregunta> preguntas)
    {
        _preguntas.Clear();
        _preguntas.AddRange(preguntas);
    }
}
