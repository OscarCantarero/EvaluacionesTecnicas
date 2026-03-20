using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Application.Common.Interfaces;
using TechEval.Domain.Resultados;
using TechEval.Domain.Resultados.Repositorios;

namespace TechEval.Application.Resultados.Commands.GenerarPDF;

public sealed record GenerarPDFCommand(
    Guid SesionId
) : IRequest<GenerarPDFResponse>;

public sealed record GenerarPDFResponse(
    bool ExitoGeneration,
    string UrlPDF,
    string Mensaje
);

public sealed class GenerarPDFCommandHandler : IRequestHandler<GenerarPDFCommand, GenerarPDFResponse>
{
    private readonly IRepositorioResultadoEvaluacion _repositorio;
    private readonly IServicioGeneradorPDF _generadorPDF;
    private readonly IServicioArchivos _servicioArchivos;

    public GenerarPDFCommandHandler(
        IRepositorioResultadoEvaluacion repositorio,
        IServicioGeneradorPDF generadorPDF,
        IServicioArchivos servicioArchivos)
    {
        _repositorio = repositorio;
        _generadorPDF = generadorPDF;
        _servicioArchivos = servicioArchivos;
    }

    public async Task<GenerarPDFResponse> Handle(GenerarPDFCommand request, CancellationToken cancellationToken)
    {
        var resultado = await _repositorio.ObtenerPorSesionAsync(request.SesionId, cancellationToken)
            ?? throw new NotFoundException(nameof(ResultadoEvaluacion), request.SesionId);

        try
        {
            // Generar contenido PDF
            var contenidoPDF = await _generadorPDF.GenerarPDFResultadoAsync(resultado, cancellationToken);

            // Guardar archivo usando el servicio de archivos
            var nombreArchivo = $"resultado_{resultado.CandidatoId}_{resultado.SesionId:N}.pdf";
            var carpeta = $"resultados/{resultado.EvaluacionId:N}";
            
            using var stream = new MemoryStream(contenidoPDF);
            var rutaArchivo = await _servicioArchivos.GuardarAsync(
                contenido: stream,
                nombreArchivo: nombreArchivo,
                carpeta: carpeta,
                cancellationToken: cancellationToken
            );
            
            var urlPDF = _servicioArchivos.ObtenerUrlPublica(rutaArchivo);

            return new GenerarPDFResponse(
                ExitoGeneration: true,
                UrlPDF: urlPDF,
                Mensaje: "PDF generado exitosamente"
            );
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Error al generar PDF: {ex.Message}", ex);
        }
    }
}
