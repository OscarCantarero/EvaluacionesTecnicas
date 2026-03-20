using TechEval.Domain.Common.ValueObjects;
using TechEval.Domain.Common.Errors;

namespace TechEval.Domain.Evaluaciones;

public sealed class OpcionRespuesta
{
    public Guid Id { get; private set; }
    public Guid PreguntaId { get; private set; }
    public string Texto { get; private set; } = string.Empty;
    public int? Puntuacion { get; private set; }
    public bool EsRevisionManual { get; private set; }
    public int Orden { get; private set; }

    private OpcionRespuesta() { }

    internal static OpcionRespuesta Crear(Guid preguntaId, string texto, int? puntuacion, bool esRevisionManual, int orden)
    {
        if (puntuacion.HasValue && esRevisionManual)
            throw ErroresEvaluacion.PuntuacionConRevisionManual.ToException();

        return new OpcionRespuesta
        {
            Id = Guid.NewGuid(),
            PreguntaId = preguntaId,
            Texto = texto,
            Puntuacion = puntuacion,
            EsRevisionManual = esRevisionManual,
            Orden = orden
        };
    }

    internal void Actualizar(string texto, int? puntuacion, bool esRevisionManual)
    {
        if (puntuacion.HasValue && esRevisionManual)
            throw ErroresEvaluacion.PuntuacionConRevisionManual.ToException();

        Texto = texto;
        Puntuacion = puntuacion;
        EsRevisionManual = esRevisionManual;
    }
}
