using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Application.Common.Interfaces;
using TechEval.Application.Sesiones.Commands.RegistrarRespuesta;
using TechEval.Domain.Evaluaciones.Repositorios;
using TechEval.Domain.Resultados;
using TechEval.Domain.Resultados.Repositorios;

namespace TechEval.Application.Resultados.Commands.EvaluarTranscripcionConIA;

public sealed class EvaluarTranscripcionConIACommandHandler
    : IRequestHandler<EvaluarTranscripcionConIACommand, EvaluarTranscripcionConIAResponse>
{
    private readonly IRepositorioResultadoEvaluacion _repositorio;
    private readonly IServicioEvaluacionIA _servicioIA;
    private readonly IRepositorioEvaluacion _repoEvaluacion;

    public EvaluarTranscripcionConIACommandHandler(
        IRepositorioResultadoEvaluacion repositorio,
        IServicioEvaluacionIA servicioIA,
        IRepositorioEvaluacion repoEvaluacion)
    {
        _repositorio = repositorio;
        _servicioIA = servicioIA;
        _repoEvaluacion = repoEvaluacion;
    }

    public async Task<EvaluarTranscripcionConIAResponse> Handle(
        EvaluarTranscripcionConIACommand request, CancellationToken cancellationToken)
    {
        var resultado = await _repositorio.ObtenerPorSesionAsync(request.SesionId, cancellationToken)
            ?? throw new NotFoundException(nameof(ResultadoEvaluacion), request.SesionId);

        var transcripcion = resultado.Transcripciones.FirstOrDefault(t => t.Id == request.TranscripcionId)
            ?? throw new NotFoundException(nameof(TranscripcionEvaluacion), request.TranscripcionId);

        if (string.IsNullOrWhiteSpace(transcripcion.Contenido))
            throw new InvalidOperationException(
                "La transcripción no tiene contenido de texto. Solo se pueden evaluar con IA transcripciones con texto.");

        var contenido = transcripcion.Contenido;

        // Reutilizar el servicio IA: tratar la transcripción como pregunta+respuesta
        var solicitud = new SolicitudEvaluacionIA(
            PreguntaId: request.TranscripcionId,
            TextoPregunta: $"Evalúa la siguiente transcripción ({transcripcion.Tipo}) en una escala de 0 a 100.",
            RespuestaCandidato: contenido,
            ContextoEvaluador: request.ContextoEvaluador,
            EscalaPuntuacion: 100,
            Rubrica: null
        );

        var respuesta = await _servicioIA.EvaluarRespuestaAsync(solicitud, cancellationToken);

        // PuntajeSugerido viene en escala 100; normalizar y registrar
        var puntaje = Math.Round(respuesta.PuntajeSugerido, 2);
        transcripcion.RegistrarEvaluacionIA(puntaje, respuesta.Justificacion);

        // Recalcular puntuación combinada
        var evaluacion = await _repoEvaluacion.ObtenerConPreguntasAsync(resultado.EvaluacionId, cancellationToken);
        resultado.CalcularPuntuacionTotal(RegistrarRespuestaCommandHandler.CalcularPuntuacionMaximaEvaluacion(evaluacion));
        await _repositorio.ActualizarAsync(resultado, cancellationToken);

        return new EvaluarTranscripcionConIAResponse(
            TranscripcionId: transcripcion.Id,
            PuntajeIA: puntaje,
            Justificacion: respuesta.Justificacion,
            NuevoPorcentajeObtenido: resultado.PorcentajeObtenido
        );
    }
}
