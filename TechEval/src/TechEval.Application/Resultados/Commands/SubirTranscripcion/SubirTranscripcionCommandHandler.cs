using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Resultados;
using TechEval.Domain.Resultados.Repositorios;

namespace TechEval.Application.Resultados.Commands.SubirTranscripcion;

public sealed class SubirTranscripcionCommandHandler : IRequestHandler<SubirTranscripcionCommand, SubirTranscripcionResponse>
{
    private readonly IRepositorioResultadoEvaluacion _repositorio;

    public SubirTranscripcionCommandHandler(IRepositorioResultadoEvaluacion repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<SubirTranscripcionResponse> Handle(SubirTranscripcionCommand request, CancellationToken cancellationToken)
    {
        var resultado = await _repositorio.ObtenerPorSesionAsync(request.SesionId, cancellationToken)
            ?? throw new NotFoundException(nameof(ResultadoEvaluacion), request.SesionId);

        var transcripcion = TranscripcionEvaluacion.Crear(request.Tipo, request.Contenido, request.UrlArchivo);
        resultado.AgregarTranscripcion(transcripcion);
        await _repositorio.ActualizarAsync(resultado, cancellationToken);

        return new SubirTranscripcionResponse(
            transcripcion.Id,
            transcripcion.Estado.ToString(),
            transcripcion.Tipo.ToString()
        );
    }
}
