using TechEval.Domain.Common.ValueObjects;
using TechEval.Domain.Common.Errors;

namespace TechEval.Domain.Evaluaciones;

public sealed class Pregunta
{
    private readonly List<OpcionRespuesta> _opciones = [];

    public Guid Id { get; private set; }
    public Guid EvaluacionId { get; private set; }
    public string Texto { get; private set; } = string.Empty;
    public TipoPregunta TipoPregunta { get; private set; }
    public NivelDificultad NivelDificultad { get; private set; }
    public int? LimiteTiempoSegundos { get; private set; }
    public bool PermiteAdjunto { get; private set; }
    public bool EsRevisionManual { get; private set; }
    public int Orden { get; private set; }
    public Guid? CategoriaId { get; private set; }
    public IReadOnlyCollection<OpcionRespuesta> Opciones => _opciones.AsReadOnly();

    private Pregunta() { }

    internal static Pregunta Crear(
        Guid evaluacionId,
        string texto,
        TipoPregunta tipo,
        NivelDificultad nivel,
        int? limiteTiempoSegundos,
        bool permiteAdjunto,
        bool esRevisionManual,
        int orden,
        Guid? categoriaId = null)
    {
        if (limiteTiempoSegundos.HasValue && limiteTiempoSegundos <= 0)
            throw ErroresEvaluacion.LimiteTiempoInvalido.ToException();

        return new Pregunta
        {
            Id = Guid.NewGuid(),
            EvaluacionId = evaluacionId,
            Texto = texto,
            TipoPregunta = tipo,
            NivelDificultad = nivel,
            LimiteTiempoSegundos = limiteTiempoSegundos,
            PermiteAdjunto = permiteAdjunto,
            EsRevisionManual = esRevisionManual,
            Orden = orden,
            CategoriaId = categoriaId
        };
    }

    internal void Actualizar(
        string texto,
        TipoPregunta tipo,
        NivelDificultad nivel,
        int? limiteTiempoSegundos,
        bool permiteAdjunto,
        bool esRevisionManual)
    {
        if (limiteTiempoSegundos.HasValue && limiteTiempoSegundos <= 0)
            throw ErroresEvaluacion.LimiteTiempoInvalido.ToException();

        Texto = texto;
        TipoPregunta = tipo;
        NivelDificultad = nivel;
        LimiteTiempoSegundos = limiteTiempoSegundos;
        PermiteAdjunto = permiteAdjunto;
        EsRevisionManual = esRevisionManual;
    }

    internal OpcionRespuesta AgregarOpcion(string texto, int? puntuacion, bool esRevisionManual)
    {
        if (TipoPregunta == TipoPregunta.TextoLibre)
            throw ErroresEvaluacion.OpcionEnPreguntaLibre.ToException();

        var orden = _opciones.Count + 1;
        var opcion = OpcionRespuesta.Crear(Id, texto, puntuacion, esRevisionManual, orden);
        _opciones.Add(opcion);
        return opcion;
    }

    internal void ActualizarOpcion(Guid opcionId, string texto, int? puntuacion, bool esRevisionManual)
    {
        var opcion = _opciones.FirstOrDefault(o => o.Id == opcionId)
            ?? throw ErroresEvaluacion.OpcionNoEncontrada.ToException();
        opcion.Actualizar(texto, puntuacion, esRevisionManual);
    }

    internal void EliminarOpcion(Guid opcionId)
    {
        var opcion = _opciones.FirstOrDefault(o => o.Id == opcionId)
            ?? throw ErroresEvaluacion.OpcionNoEncontrada.ToException();
        _opciones.Remove(opcion);
    }

    internal void AsignarCategoria(Guid? categoriaId)
    {
        CategoriaId = categoriaId;
    }

    // Needed for EF Core collection loading
    internal void CargarOpciones(IEnumerable<OpcionRespuesta> opciones)
    {
        _opciones.Clear();
        _opciones.AddRange(opciones);
    }
}
