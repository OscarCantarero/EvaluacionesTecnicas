using TechEval.Domain.Resultados;

namespace TechEval.Application.Common.Interfaces;

/// <summary>
/// Servicio para generar reportes en PDF
/// </summary>
public interface IServicioGeneradorPDF
{
    /// <summary>
    /// Genera PDF del resultado de evaluación
    /// </summary>
    Task<byte[]> GenerarPDFResultadoAsync(ResultadoEvaluacion resultado, CancellationToken cancellationToken = default);
}
