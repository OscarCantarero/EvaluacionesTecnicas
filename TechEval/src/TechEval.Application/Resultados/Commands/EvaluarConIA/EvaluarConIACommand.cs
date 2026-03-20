using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Application.Common.Interfaces;
using TechEval.Application.Sesiones.Commands.RegistrarRespuesta;
using TechEval.Domain.Evaluaciones.Repositorios;
using TechEval.Domain.Resultados;
using TechEval.Domain.Resultados.Repositorios;
using TechEval.Domain.Sesiones.Repositorios;

namespace TechEval.Application.Resultados.Commands.EvaluarConIA;

/// <summary>
/// Evalúa una respuesta de candidato usando IA.
/// El evaluador define la escala (ej: 10 = puntaje sobre 10) y el contexto/rol
/// que la IA debe asumir para calificar.
/// </summary>
public sealed record EvaluarConIACommand(
    Guid SesionId,
    Guid PreguntaId,
    string ContextoEvaluador,
    int EscalaPuntuacion = 10,
    string? Rubrica = null
) : IRequest<EvaluarConIAResponse>;

public sealed record EvaluarConIAResponse(
    Guid PreguntaId,
    decimal PuntajeSugerido,
    int EscalaPuntuacion,
    string Justificacion,
    bool RequiereRevisionManual,
    List<string> AspectosPositivos,
    List<string> AspectosNegativos
);

public sealed class EvaluarConIACommandHandler : IRequestHandler<EvaluarConIACommand, EvaluarConIAResponse>
{
    private readonly IRepositorioResultadoEvaluacion _repositorio;
    private readonly IRepositorioSesionEvaluacion _repoSesion;
    private readonly IRepositorioEvaluacion _repoEvaluacion;
    private readonly IServicioEvaluacionIA _servicioIA;

    public EvaluarConIACommandHandler(
        IRepositorioResultadoEvaluacion repositorio,
        IRepositorioSesionEvaluacion repoSesion,
        IRepositorioEvaluacion repoEvaluacion,
        IServicioEvaluacionIA servicioIA)
    {
        _repositorio = repositorio;
        _repoSesion = repoSesion;
        _repoEvaluacion = repoEvaluacion;
        _servicioIA = servicioIA;
    }

    public async Task<EvaluarConIAResponse> Handle(EvaluarConIACommand request, CancellationToken cancellationToken)
    {
        var resultado = await _repositorio.ObtenerPorSesionAsync(request.SesionId, cancellationToken)
            ?? throw new NotFoundException(nameof(ResultadoEvaluacion), request.SesionId);

        var puntuacion = resultado.ObtenerPuntuacion(request.PreguntaId)
            ?? throw new NotFoundException(nameof(PuntuacionPregunta), request.PreguntaId);

        // Obtener el texto original de la pregunta
        var sesion = await _repoSesion.ObtenerConPreguntasAsync(request.SesionId, cancellationToken);
        var preguntaSesion = sesion?.Preguntas.FirstOrDefault(p => p.Id == request.PreguntaId);
        var textoPregunta = preguntaSesion?.Texto ?? "Pregunta no disponible";

        // Construir solicitud estructurada para el servicio de IA
        var solicitud = new SolicitudEvaluacionIA(
            PreguntaId: request.PreguntaId,
            TextoPregunta: textoPregunta,
            RespuestaCandidato: puntuacion.Respuesta,
            ContextoEvaluador: request.ContextoEvaluador,
            EscalaPuntuacion: request.EscalaPuntuacion,
            Rubrica: request.Rubrica
        );

        var respuestaIA = await _servicioIA.EvaluarRespuestaAsync(solicitud, cancellationToken);

        // Registrar sugerencia de IA en la puntuación
        puntuacion.RegistrarSugerenciaIA(
            respuestaIA.PuntajeSugerido,
            respuestaIA.Justificacion
        );

        // Actualizar resultado
        var evaluacion = await _repoEvaluacion.ObtenerConPreguntasAsync(resultado.EvaluacionId, cancellationToken);
        resultado.CalcularPuntuacionTotal(RegistrarRespuestaCommandHandler.CalcularPuntuacionMaximaEvaluacion(evaluacion));
        await _repositorio.ActualizarAsync(resultado, cancellationToken);

        return new EvaluarConIAResponse(
            PreguntaId: request.PreguntaId,
            PuntajeSugerido: respuestaIA.PuntajeSugerido,
            EscalaPuntuacion: respuestaIA.EscalaPuntuacion,
            Justificacion: respuestaIA.Justificacion,
            RequiereRevisionManual: respuestaIA.RequiereRevisionManual,
            AspectosPositivos: respuestaIA.AspectosPositivos,
            AspectosNegativos: respuestaIA.AspectosNegativos
        );
    }
}
