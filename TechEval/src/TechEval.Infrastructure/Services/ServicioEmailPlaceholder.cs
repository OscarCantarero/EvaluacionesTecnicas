using Microsoft.Extensions.Logging;
using TechEval.Application.Common.Interfaces;

namespace TechEval.Infrastructure.Services;

/// <summary>
/// Implementación placeholder del servicio de email.
/// En producción, reemplazar por SMTP/SendGrid/SES.
/// </summary>
public sealed class ServicioEmailPlaceholder(ILogger<ServicioEmailPlaceholder> logger) : IServicioEmail
{
    public Task EnviarAsync(
        string destinatario,
        string asunto,
        string cuerpoHtml,
        byte[]? adjuntoPdf = null,
        string? nombreAdjunto = null,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[EMAIL PLACEHOLDER] Para: {Destinatario} | Asunto: {Asunto} | Adjunto: {Adjunto} | Body length: {Length}",
            destinatario, asunto, nombreAdjunto ?? "ninguno", cuerpoHtml.Length);

        return Task.CompletedTask;
    }
}
