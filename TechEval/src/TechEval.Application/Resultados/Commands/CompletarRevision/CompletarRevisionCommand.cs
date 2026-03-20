using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Application.Common.Interfaces;
using TechEval.Domain.Resultados;
using TechEval.Domain.Resultados.Repositorios;

namespace TechEval.Application.Resultados.Commands.CompletarRevision;

public sealed record CompletarRevisionCommand(
    Guid SesionId,
    string EvaluadorId
) : IRequest<CompletarRevisionResponse>;

public sealed record CompletarRevisionResponse(
    bool Exito,
    string EstadoRevision,
    string Mensaje,
    bool EmailEnviado
);

public sealed class CompletarRevisionCommandHandler : IRequestHandler<CompletarRevisionCommand, CompletarRevisionResponse>
{
    private readonly IRepositorioResultadoEvaluacion _repositorio;
    private readonly IServicioGeneradorPDF _generadorPDF;
    private readonly IServicioEmail _servicioEmail;
    private readonly IServicioAuth _servicioAuth;
    private readonly IServicioArchivos _servicioArchivos;

    public CompletarRevisionCommandHandler(
        IRepositorioResultadoEvaluacion repositorio,
        IServicioGeneradorPDF generadorPDF,
        IServicioEmail servicioEmail,
        IServicioAuth servicioAuth,
        IServicioArchivos servicioArchivos)
    {
        _repositorio = repositorio;
        _generadorPDF = generadorPDF;
        _servicioEmail = servicioEmail;
        _servicioAuth = servicioAuth;
        _servicioArchivos = servicioArchivos;
    }

    public async Task<CompletarRevisionResponse> Handle(CompletarRevisionCommand request, CancellationToken cancellationToken)
    {
        var resultado = await _repositorio.ObtenerPorSesionAsync(request.SesionId, cancellationToken)
            ?? throw new NotFoundException(nameof(ResultadoEvaluacion), request.SesionId);

        // Iniciar revisión si aún no se ha hecho
        if (resultado.EstadoRevision == Domain.Resultados.EstadoRevision.PendienteRevision)
        {
            resultado.IniciarRevision(request.EvaluadorId);
        }

        // Completar revisión
        resultado.CompletarRevision();
        await _repositorio.ActualizarAsync(resultado, cancellationToken);

        // Generar PDF y enviar email al candidato
        var emailEnviado = false;
        try
        {
            var pdfBytes = await _generadorPDF.GenerarPDFResultadoAsync(resultado, cancellationToken);

            // Guardar PDF
            var nombreArchivo = $"resultado_{resultado.CandidatoId}_{resultado.SesionId:N}.pdf";
            using var stream = new MemoryStream(pdfBytes);
            await _servicioArchivos.GuardarAsync(stream, $"resultados/{resultado.EvaluacionId:N}", nombreArchivo, cancellationToken);

            // Obtener email del candidato
            var usuarios = await _servicioAuth.ListarUsuariosAsync(null, cancellationToken);
            var candidato = usuarios.FirstOrDefault(u => u.Id == resultado.CandidatoId);

            if (candidato != null)
            {
                var cuerpoHtml = GenerarCuerpoEmail(resultado);
                await _servicioEmail.EnviarAsync(
                    destinatario: candidato.Email,
                    asunto: $"Resultados de tu evaluación: {resultado.TituloEvaluacion}",
                    cuerpoHtml: cuerpoHtml,
                    adjuntoPdf: pdfBytes,
                    nombreAdjunto: nombreArchivo,
                    cancellationToken: cancellationToken);
                emailEnviado = true;
            }
        }
        catch
        {
            // El email es best-effort; no debe fallar la revisión
        }

        return new CompletarRevisionResponse(
            Exito: true,
            EstadoRevision: resultado.EstadoRevision.ToString(),
            Mensaje: emailEnviado
                ? "Revisión completada y resultados enviados al candidato"
                : "Revisión completada (email pendiente de envío)",
            EmailEnviado: emailEnviado
        );
    }

    private static string GenerarCuerpoEmail(ResultadoEvaluacion resultado)
    {
        return $"""
            <html>
            <body style="font-family: Arial, sans-serif; color: #333;">
                <h2>Resultados de Evaluación: {resultado.TituloEvaluacion}</h2>
                <p>Hola <strong>{resultado.NombreCandidato}</strong>,</p>
                <p>Tu evaluación ha sido revisada. A continuación el resumen:</p>
                <table style="border-collapse: collapse; width: 100%; max-width: 400px;">
                    <tr><td style="padding: 8px; border: 1px solid #ddd;"><strong>Puntuación Total</strong></td><td style="padding: 8px; border: 1px solid #ddd;">{resultado.PuntuacionTotal:F1}</td></tr>
                    <tr><td style="padding: 8px; border: 1px solid #ddd;"><strong>Porcentaje</strong></td><td style="padding: 8px; border: 1px solid #ddd;">{resultado.PorcentajeObtenido:F1}%</td></tr>
                    <tr><td style="padding: 8px; border: 1px solid #ddd;"><strong>Resultado</strong></td><td style="padding: 8px; border: 1px solid #ddd;">{resultado.ObtenerEstadoGeneral()}</td></tr>
                </table>
                <p>Adjunto encontrarás el reporte detallado en PDF.</p>
                <p style="color: #888; font-size: 12px;">Este es un mensaje automático de TechEval.</p>
            </body>
            </html>
            """;
    }
}
