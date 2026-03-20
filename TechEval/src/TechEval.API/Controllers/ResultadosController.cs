using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechEval.Application.Common.Interfaces;
using TechEval.Application.Resultados.Commands.AsignarPuntuacion;
using TechEval.Application.Resultados.Commands.CompletarRevision;
using TechEval.Application.Resultados.Commands.EvaluarConIA;
using TechEval.Application.Resultados.Commands.EvaluarTranscripcionConIA;
using TechEval.Application.Resultados.Commands.GenerarPDF;
using TechEval.Application.Resultados.Commands.SubirTranscripcion;
using TechEval.Application.Resultados.Queries.ObtenerResultados;
using TechEval.Domain.Resultados;
using MediatR;

namespace TechEval.API.Controllers;

/// <summary>
/// Endpoints para resultados y revisión de evaluaciones
/// 
/// Endpoints:
/// - GET /api/resultados/{sesionId}: Obtener resultados de sesión
/// - POST /api/resultados/{sesionId}/pdf: Generar PDF
/// - PATCH /api/resultados/{sesionId}/puntuaciones/{preguntaId}: Asignar puntaje manual
/// - POST /api/resultados/{sesionId}/ia-evaluate: Evaluar con IA (placeholder)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ResultadosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IServicioArchivos _servicioArchivos;

    public ResultadosController(IMediator mediator, IServicioArchivos servicioArchivos)
    {
        _mediator = mediator;
        _servicioArchivos = servicioArchivos;
    }

    /// <summary>
    /// Obtiene los resultados completos de una sesión de evaluación
    /// </summary>
    /// <param name="sesionId">ID de la sesión</param>
    [HttpGet("{sesionId:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerResultados(Guid sesionId)
    {
        var query = new ObtenerResultadosQuery(sesionId);
        var resultado = await _mediator.Send(query);
        return Ok(resultado);
    }

    /// <summary>
    /// Genera un PDF con los resultados de la evaluación
    /// </summary>
    /// <param name="sesionId">ID de la sesión</param>
    [HttpPost("{sesionId:guid}/pdf")]
    [Authorize(Roles = "Evaluador, Administrador")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerarPDF(Guid sesionId)
    {
        var command = new GenerarPDFCommand(sesionId);
        var resultado = await _mediator.Send(command);
        return Ok(resultado);
    }

    /// <summary>
    /// Asigna una puntuación manual a una pregunta (evaluador)
    /// </summary>
    /// <param name="sesionId">ID de la sesión</param>
    /// <param name="request">Datos de la puntuación</param>
    [HttpPatch("{sesionId:guid}/puntuaciones")]
    [Authorize(Roles = "Evaluador, Administrador")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> AsignarPuntuacion(Guid sesionId, [FromBody] AsignarPuntuacionRequest request)
    {
        var command = new AsignarPuntuacionCommand(
            SesionId: sesionId,
            PreguntaId: request.PreguntaId,
            Puntaje: request.Puntaje,
            Observaciones: request.Observaciones
        );

        var resultado = await _mediator.Send(command);
        return Ok(resultado);
    }

    /// <summary>
    /// Evalúa una respuesta libre con IA.
    /// El evaluador define la escala de puntuación y el contexto/rol que la IA asumirá.
    /// </summary>
    /// <param name="sesionId">ID de la sesión</param>
    /// <param name="request">Datos para evaluación con IA</param>
    [HttpPost("{sesionId:guid}/ia-evaluate")]
    [Authorize(Roles = "Evaluador, Administrador")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EvaluarConIA(Guid sesionId, [FromBody] EvaluarConIARequest request)
    {
        var command = new EvaluarConIACommand(
            SesionId: sesionId,
            PreguntaId: request.PreguntaId,
            ContextoEvaluador: request.ContextoEvaluador,
            EscalaPuntuacion: request.EscalaPuntuacion,
            Rubrica: request.Rubrica
        );

        var resultado = await _mediator.Send(command);
        return Ok(resultado);
    }

    /// <summary>
    /// Acepta la sugerencia de puntuación de la IA y la convierte en puntuación manual definitiva.
    /// </summary>
    [HttpPost("{sesionId:guid}/ia-aceptar")]
    [Authorize(Roles = "Evaluador, Administrador")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> AceptarSugerenciaIA(Guid sesionId, [FromBody] DecisionIARequest request)
    {
        var resultado = await _mediator.Send(new AsignarPuntuacionCommand(
            SesionId: sesionId,
            PreguntaId: request.PreguntaId,
            Puntaje: request.PuntajeAceptado ?? 0,
            Observaciones: "Puntaje aceptado de sugerencia IA"
        ));

        return Ok(resultado);
    }

    /// <summary>
    /// Completa la revisión de una sesión, genera PDF y envía resultados al candidato por email.
    /// </summary>
    [HttpPost("{sesionId:guid}/completar-revision")]
    [Authorize(Roles = "Evaluador, Administrador")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompletarRevision(Guid sesionId)
    {
        var contexto = HttpContext.RequestServices.GetRequiredService<IContextoUsuario>();
        var command = new CompletarRevisionCommand(sesionId, contexto.UsuarioId);
        var resultado = await _mediator.Send(command);
        return Ok(resultado);
    }

    /// <summary>
    /// Rechaza la sugerencia de IA. La puntuación IA queda registrada pero no se aplica.
    /// El evaluador debe asignar manualmente.
    /// </summary>
    [HttpPost("{sesionId:guid}/ia-rechazar")]
    [Authorize(Roles = "Evaluador, Administrador")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> RechazarSugerenciaIA(Guid sesionId, [FromBody] DecisionIARequest request)
    {
        // Solo registrar el rechazo como observación; no se aplica puntaje
        var resultado = await _mediator.Send(new AsignarPuntuacionCommand(
            SesionId: sesionId,
            PreguntaId: request.PreguntaId,
            Puntaje: request.PuntajeManualAlternativo ?? 0,
            Observaciones: $"Sugerencia IA rechazada. {request.MotivoRechazo}"
        ));

        return Ok(resultado);
    }

    /// <summary>
    /// Sube una transcripción (texto o archivo) para una sesión.
    /// </summary>
    /// <param name="sesionId">ID de la sesión</param>
    /// <param name="request">Datos de la transcripción</param>
    [HttpPost("{sesionId:guid}/transcripciones")]
    [Authorize(Roles = "Evaluador,Administrador")]
    [Consumes("multipart/form-data")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubirTranscripcion(Guid sesionId, [FromForm] SubirTranscripcionRequest request)
    {
        string? urlArchivo = null;
        if (request.Archivo != null && request.Archivo.Length > 0)
        {
            var ruta = await _servicioArchivos.GuardarAsync(
                request.Archivo.OpenReadStream(),
                request.Archivo.FileName,
                $"transcripciones/{sesionId}",
                cancellationToken: HttpContext.RequestAborted);
            urlArchivo = _servicioArchivos.ObtenerUrlPublica(ruta);
        }

        if (!Enum.TryParse<TipoTranscripcion>(request.Tipo, ignoreCase: true, out var tipo))
            return BadRequest($"Tipo de transcripción inválido: '{request.Tipo}'. Valores válidos: Sesion, Entrevista.");

        var command = new SubirTranscripcionCommand(sesionId, tipo, request.Contenido, urlArchivo);
        var resultado = await _mediator.Send(command);
        return Ok(resultado);
    }

    /// <summary>
    /// Evalúa una transcripción específica con IA y actualiza el scoring combinado.
    /// </summary>
    /// <param name="sesionId">ID de la sesión</param>
    /// <param name="transcripcionId">ID de la transcripción</param>
    /// <param name="request">Contexto para la evaluación IA</param>
    [HttpPost("{sesionId:guid}/transcripciones/{transcripcionId:guid}/evaluar-ia")]
    [Authorize(Roles = "Evaluador,Administrador")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EvaluarTranscripcionConIA(
        Guid sesionId,
        Guid transcripcionId,
        [FromBody] EvaluarTranscripcionConIARequest request)
    {
        var command = new EvaluarTranscripcionConIACommand(sesionId, transcripcionId, request.ContextoEvaluador);
        var resultado = await _mediator.Send(command);
        return Ok(resultado);
    }
}

public sealed record AsignarPuntuacionRequest(
    Guid PreguntaId,
    decimal Puntaje,
    string? Observaciones = null
);

public sealed record EvaluarConIARequest(
    Guid PreguntaId,
    string ContextoEvaluador,
    int EscalaPuntuacion = 10,
    string? Rubrica = null
);

public sealed record DecisionIARequest(
    Guid PreguntaId,
    decimal? PuntajeAceptado = null,
    decimal? PuntajeManualAlternativo = null,
    string? MotivoRechazo = null
);

public sealed record SubirTranscripcionRequest(
    string Tipo,
    string? Contenido = null,
    IFormFile? Archivo = null
);

public sealed record EvaluarTranscripcionConIARequest(
    string ContextoEvaluador
);
