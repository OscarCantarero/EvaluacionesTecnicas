using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechEval.Application.Sesiones.Commands.AgregarAdjuntoRespuesta;
using TechEval.Application.Sesiones.Commands.CrearSesion;
using TechEval.Application.Sesiones.Commands.CrearSesionesMasivas;
using TechEval.Application.Sesiones.Commands.IniciarSesion;
using TechEval.Application.Sesiones.Commands.RegistrarRespuesta;
using TechEval.Application.Sesiones.Commands.RegistrarViolacionPestana;
using TechEval.Application.Sesiones.Queries.ListarSesionesCandidato;
using TechEval.Application.Sesiones.Queries.ObtenerSesion;
using MediatR;

namespace TechEval.API.Controllers;

/// <summary>
/// Operaciones para sesiones de evaluación.
/// 
/// Endpoints:
/// - GET /api/sesiones/{id}: Obtener sesión por ID
/// - GET /api/sesiones/candidato: Listar sesiones del candidato
/// - POST /api/sesiones: Evaluador crea nueva sesión
/// - POST /api/sesiones/{id}/iniciar: Candidato inicia sesión
/// - POST /api/sesiones/{id}/respuestas: Candidato envía respuesta
/// - POST /api/sesiones/{id}/violaciones-pestana: Registro de violaciones
/// - POST /api/sesiones/{id}/adjuntos: Registrar archivo adjunto
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class SesionesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SesionesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene una sesión específica con todas sus preguntas y respuestas.
    /// </summary>
    /// <param name="id">ID de la sesión</param>
    [HttpGet("{id:guid}")]
    [Produces("application/json")]
    public async Task<IActionResult> ObtenerSesion(Guid id)
    {
        var query = new ObtenerSesionQuery(id);
        var resultado = await _mediator.Send(query);
        return Ok(resultado);
    }

    /// <summary>
    /// Lista sesiones con paginación y filtros opcionales.
    /// </summary>
    [HttpGet("candidato")]
    [Produces("application/json")]
    public async Task<IActionResult> ListarSesionesCandidato(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10,
        [FromQuery] string? estado = null,
        [FromQuery] string? busqueda = null)
    {
        var query = new ListarSesionesCandidatoQuery(pagina, tamanoPagina, estado, busqueda);
        var resultado = await _mediator.Send(query);
        return Ok(resultado);
    }

    /// <summary>
    /// Crea una nueva sesión de evaluación (solo evaluadores).
    /// Retorna el código de acceso para el candidato.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Evaluador,Administrador")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> CrearSesion([FromBody] CrearSesionRequest request)
    {
        var command = new CrearSesionCommand(
            EvaluacionId: request.EvaluacionId,
            CandidatoId: request.CandidatoId
        );

        var resultado = await _mediator.Send(command);
        
        return CreatedAtAction(nameof(ObtenerSesion), 
            new { id = resultado.SesionId }, 
            resultado);
    }

    /// <summary>
    /// Crea sesiones masivas para múltiples candidatos en una evaluación.
    /// </summary>
    [HttpPost("masivas")]
    [Authorize(Roles = "Evaluador,Administrador")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> CrearSesionesMasivas([FromBody] CrearSesionesMasivasRequest request)
    {
        var command = new CrearSesionesMasivasCommand(
            EvaluacionId: request.EvaluacionId,
            CandidatoIds: request.CandidatoIds
        );

        var resultado = await _mediator.Send(command);
        return Ok(resultado);
    }

    /// <summary>
    /// Candidato inicia la sesión usando su código de acceso.
    /// Retorna la primera pregunta.
    /// </summary>
    /// <param name="id">ID de la sesión</param>
    /// <param name="request">Código de acceso del candidato</param>
    [HttpPost("{id:guid}/iniciar")]
    [Authorize(Roles = "Candidato")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> IniciarSesion(Guid id, [FromBody] IniciarSesionRequest request)
    {
        var command = new IniciarSesionCommand(
            CodigoAcceso: request.CodigoAcceso
        );

        var resultado = await _mediator.Send(command);
        return Ok(resultado);
    }

    /// <summary>
    /// Candidato envía respuesta a una pregunta.
    /// </summary>
    /// <param name="id">ID de la sesión</param>
    /// <param name="request">Datos de la respuesta del candidato</param>
    [HttpPost("{id:guid}/respuestas")]
    [AllowAnonymous]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> RegistrarRespuesta(Guid id, [FromBody] RegistrarRespuestaRequest request)
    {
        var command = new RegistrarRespuestaCommand(
            SesionId: id,
            PreguntaId: request.PreguntaId,
            Texto: request.Texto,
            TiempoEmpleadoSegundos: request.TiempoEmpleadoSegundos,
            FueExpirado: request.FueExpirado
        );

        var resultado = await _mediator.Send(command);
        return Ok(resultado);
    }

    /// <summary>
    /// Registra una violación de pestaña detectada.
    /// </summary>
    /// <param name="id">ID de la sesión</param>
    [HttpPost("{id:guid}/violaciones-pestana")]
    [AllowAnonymous]
    [Produces("application/json")]
    public async Task<IActionResult> RegistrarViolacionPestana(Guid id)
    {
        var command = new RegistrarViolacionPestanaCommand(SesionId: id);
        await _mediator.Send(command);
        return Ok(new { mensaje = "Violación registrada" });
    }

    /// <summary>
    /// Agrega un archivo adjunto a una respuesta.
    /// </summary>
    /// <param name="id">ID de la sesión</param>
    /// <param name="request">Archivo adjunto y pregunta asociada</param>
    [HttpPost("{id:guid}/adjuntos")]
    [AllowAnonymous]
    [Produces("application/json")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AgregarAdjunto(Guid id, [FromForm] AgregarAdjuntoRequest request)
    {
        if (request.Archivo == null || request.Archivo.Length == 0)
            return BadRequest("El archivo no puede estar vacío");

        // Aquí iría la lógica para guardar el archivo y obtener la URL
        // Por ahora, simulamos una URL
        var urlAdjunto = $"https://storage.example.com/sesiones/{id}/adjuntos/{Guid.NewGuid()}.pdf";

        var command = new AgregarAdjuntoRespuestaCommand(
            SesionId: id,
            PreguntaId: request.PreguntaId,
            UrlAdjunto: urlAdjunto
        );

        await _mediator.Send(command);
        return Ok(new { mensaje = "Adjunto registrado", urlAdjunto });
    }
}

public sealed record CrearSesionRequest(
    Guid EvaluacionId,
    string CandidatoId
);

public sealed record IniciarSesionRequest(
    string CodigoAcceso
);

public sealed record RegistrarRespuestaRequest(
    Guid PreguntaId,
    string Texto,
    int TiempoEmpleadoSegundos,
    bool FueExpirado
);

public sealed record AgregarAdjuntoRequest(
    Guid PreguntaId,
    IFormFile Archivo
);

public sealed record CrearSesionesMasivasRequest(
    Guid EvaluacionId,
    List<string> CandidatoIds
);
