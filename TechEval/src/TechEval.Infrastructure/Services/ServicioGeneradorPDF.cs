using System.Text;
using TechEval.Application.Common.Interfaces;
using TechEval.Domain.Resultados;

namespace TechEval.Infrastructure.Services;

/// <summary>
/// Implementación del generador PDF usando un enfoque simple
/// (Para producción, usar QuestPDF cuando esté agregado al proyecto)
/// </summary>
public sealed class ServicioGeneradorPDF : IServicioGeneradorPDF
{
    public async Task<byte[]> GenerarPDFResultadoAsync(ResultadoEvaluacion resultado, CancellationToken cancellationToken = default)
    {
        // Placeholder: Generar contenido de PDF simple en HTML
        // Esto será reemplazado con QuestPDF cuando se agregue la librería
        
        var contenidoHTML = GenerarHTMLResultado(resultado);
        
        // Por ahora, convertir HTML a bytes (versión simplificada)
        // Con QuestPDF: QuestPDF.Settings.CheckLicense(true);
        // var documento = Document.Create(container => { ... });
        // return documento.GeneratePdf();

        var bytes = Encoding.UTF8.GetBytes(contenidoHTML);
        
        return await Task.FromResult(bytes);
    }

    private string GenerarHTMLResultado(ResultadoEvaluacion resultado)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang='es'>");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset='UTF-8'>");
        sb.AppendLine("  <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
        sb.AppendLine("  <title>Resultado de Evaluación</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine("    body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 20px; }");
        sb.AppendLine("    .header { background: #0066cc; color: white; padding: 20px; border-radius: 5px; }");
        sb.AppendLine("    .section { margin: 20px 0; border: 1px solid #ddd; padding: 15px; border-radius: 5px; }");
        sb.AppendLine("    .score-box { display: inline-block; background: #f0f0f0; padding: 20px; border-radius: 5px; margin: 10px 0; }");
        sb.AppendLine("    .question { margin: 10px 0; padding: 10px; background: #f9f9f9; border-left: 4px solid #0066cc; }");
        sb.AppendLine("    .excellent { color: #28a745; font-weight: bold; }");
        sb.AppendLine("    .good { color: #17a2b8; font-weight: bold; }");
        sb.AppendLine("    .fair { color: #ffc107; font-weight: bold; }");
        sb.AppendLine("    .poor { color: #dc3545; font-weight: bold; }");
        sb.AppendLine("    table { width: 100%; border-collapse: collapse; margin: 10px 0; }");
        sb.AppendLine("    th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        sb.AppendLine("    th { background-color: #0066cc; color: white; }");
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        // Header
        sb.AppendLine("  <div class='header'>");
        sb.AppendLine("    <h1>Resultado de Evaluación</h1>");
        sb.AppendLine($"    <p>Evaluación: <strong>{resultado.TituloEvaluacion}</strong></p>");
        sb.AppendLine($"    <p>Candidato: <strong>{resultado.NombreCandidato}</strong></p>");
        sb.AppendLine($"    <p>Fecha: <strong>{resultado.FechaCompletacion:dd/MM/yyyy HH:mm}</strong></p>");
        sb.AppendLine("  </div>");

        // Score summary
        sb.AppendLine("  <div class='section'>");
        sb.AppendLine("    <h2>Resumen de Puntuación</h2>");
        sb.AppendLine("    <div class='score-box'>");
        sb.AppendLine($"      <p><strong>Puntuación Total:</strong> {resultado.PuntuacionTotal:F2} / {resultado.PuntuacionMaxima:F2}</p>");
        
        var claseEstado = resultado.PorcentajeObtenido switch
        {
            >= 90 => "excellent",
            >= 80 => "good",
            >= 70 => "fair",
            _ => "poor"
        };
        
        sb.AppendLine($"      <p class='{claseEstado}'><strong>Desempeño:</strong> {resultado.ObtenerEstadoGeneral()} ({resultado.PorcentajeObtenido:F1}%)</p>");
        sb.AppendLine("    </div>");
        sb.AppendLine($"    <p><strong>Violaciones de Pestaña:</strong> {resultado.ViolacionesPestana}</p>");
        sb.AppendLine($"    <p><strong>Tiempo Total:</strong> {resultado.TiempoTotalSegundos / 60} minutos</p>");
        sb.AppendLine("  </div>");

        // Detalles de preguntas
        if (resultado.Puntuaciones.Count > 0)
        {
            sb.AppendLine("  <div class='section'>");
            sb.AppendLine("    <h2>Detalles por Pregunta</h2>");
            sb.AppendLine("    <table>");
            sb.AppendLine("      <thead>");
            sb.AppendLine("        <tr>");
            sb.AppendLine("          <th>Pregunta</th>");
            sb.AppendLine("          <th>Tipo</th>");
            sb.AppendLine("          <th>Respuesta</th>");
            sb.AppendLine("          <th>Puntuación</th>");
            sb.AppendLine("          <th>Tiempo</th>");
            sb.AppendLine("        </tr>");
            sb.AppendLine("      </thead>");
            sb.AppendLine("      <tbody>");

            foreach (var puntuacion in resultado.Puntuaciones.OrderBy(p => p.NumeroPregunta))
            {
                sb.AppendLine("        <tr>");
                sb.AppendLine($"          <td>Pregunta {puntuacion.NumeroPregunta}</td>");
                sb.AppendLine($"          <td>{puntuacion.TipoPregunta}</td>");
                sb.AppendLine($"          <td>{(string.IsNullOrEmpty(puntuacion.Respuesta) ? "N/A" : puntuacion.Respuesta.Substring(0, Math.Min(50, puntuacion.Respuesta.Length)))}</td>");
                sb.AppendLine($"          <td>{puntuacion.ObtenerPuntuacionFinal():F2}</td>");
                sb.AppendLine($"          <td>{puntuacion.TiempoEmpleadoSegundos}s</td>");
                sb.AppendLine("        </tr>");
            }

            sb.AppendLine("      </tbody>");
            sb.AppendLine("    </table>");
            sb.AppendLine("  </div>");
        }

        // Estado de revisión
        sb.AppendLine("  <div class='section'>");
        sb.AppendLine("    <h2>Estado de Revisión</h2>");
        sb.AppendLine($"    <p><strong>Estado:</strong> {resultado.EstadoRevision}</p>");
        if (resultado.FechaFinRevision.HasValue)
        {
            sb.AppendLine($"    <p><strong>Completada:</strong> {resultado.FechaFinRevision:dd/MM/yyyy HH:mm}</p>");
        }
        sb.AppendLine("  </div>");

        // Footer
        sb.AppendLine("  <hr>");
        sb.AppendLine("  <p style='text-align: center; color: #666; font-size: 12px;'>");
        sb.AppendLine("    Este documento fue generado automáticamente por el sistema TechEval");
        sb.AppendLine("  </p>");

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }
}
