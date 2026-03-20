namespace TechEval.Application.Common.Interfaces;

public interface IServicioEmail
{
    Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml, byte[]? adjuntoPdf = null, string? nombreAdjunto = null, CancellationToken cancellationToken = default);
}
