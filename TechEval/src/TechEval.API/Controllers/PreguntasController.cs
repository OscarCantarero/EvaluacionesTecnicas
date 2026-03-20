using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechEval.Application.Evaluaciones.Commands.AgregarOpcion;
using TechEval.Application.Evaluaciones.Commands.AgregarPregunta;
using TechEval.Application.Evaluaciones.Commands.ActualizarOpcion;
using TechEval.Application.Evaluaciones.Commands.ActualizarPregunta;
using TechEval.Application.Evaluaciones.Commands.EliminarOpcion;
using TechEval.Application.Evaluaciones.Commands.EliminarPregunta;
using TechEval.Application.Evaluaciones.Commands.AgregarPreguntasMasivas;
using TechEval.Application.Evaluaciones.Queries.ObtenerPreguntasBanco;
using TechEval.Domain.Common.ValueObjects;

namespace TechEval.API.Controllers;

[ApiController]
[Authorize(Roles = "Evaluador,Administrador")]
public sealed class PreguntasController(ISender sender) : ControllerBase
{
    /// <summary>Agrega una pregunta a una evaluación existente.</summary>
    [HttpPost("api/evaluaciones/{evaluacionId:guid}/preguntas")]
    public async Task<IActionResult> AgregarPregunta(
        Guid evaluacionId,
        [FromBody] AgregarPreguntaRequest request,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(new AgregarPreguntaCommand(
            evaluacionId, request.Texto, request.TipoPregunta, request.NivelDificultad,
            request.LimiteTiempoSegundos, request.PermiteAdjunto, request.EsRevisionManual, request.CategoriaId),
            cancellationToken);
        return Created($"api/preguntas/{id}", new { Id = id });
    }

    /// <summary>Actualiza una pregunta existente.</summary>
    [HttpPut("api/evaluaciones/{evaluacionId:guid}/preguntas/{preguntaId:guid}")]
    public async Task<IActionResult> ActualizarPregunta(
        Guid evaluacionId, Guid preguntaId,
        [FromBody] ActualizarPreguntaRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new ActualizarPreguntaCommand(
            evaluacionId, preguntaId, request.Texto, request.TipoPregunta, request.NivelDificultad,
            request.LimiteTiempoSegundos, request.PermiteAdjunto, request.EsRevisionManual, request.CategoriaId),
            cancellationToken);
        return NoContent();
    }

    /// <summary>Elimina una pregunta de una evaluación.</summary>
    [HttpDelete("api/evaluaciones/{evaluacionId:guid}/preguntas/{preguntaId:guid}")]
    public async Task<IActionResult> EliminarPregunta(
        Guid evaluacionId, Guid preguntaId,
        CancellationToken cancellationToken)
    {
        await sender.Send(new EliminarPreguntaCommand(evaluacionId, preguntaId), cancellationToken);
        return NoContent();
    }

    /// <summary>Agrega una opción de respuesta a una pregunta.</summary>
    [HttpPost("api/evaluaciones/{evaluacionId:guid}/preguntas/{preguntaId:guid}/opciones")]
    public async Task<IActionResult> AgregarOpcion(
        Guid evaluacionId, Guid preguntaId,
        [FromBody] AgregarOpcionRequest request,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(new AgregarOpcionCommand(
            evaluacionId, preguntaId, request.Texto, request.Puntuacion, request.EsRevisionManual),
            cancellationToken);
        return Created($"api/opciones/{id}", new { Id = id });
    }

    /// <summary>Actualiza una opción de respuesta.</summary>
    [HttpPut("api/evaluaciones/{evaluacionId:guid}/preguntas/{preguntaId:guid}/opciones/{opcionId:guid}")]
    public async Task<IActionResult> ActualizarOpcion(
        Guid evaluacionId, Guid preguntaId, Guid opcionId,
        [FromBody] ActualizarOpcionRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new ActualizarOpcionCommand(
            evaluacionId, preguntaId, opcionId, request.Texto, request.Puntuacion, request.EsRevisionManual),
            cancellationToken);
        return NoContent();
    }

    /// <summary>Elimina una opción de respuesta.</summary>
    [HttpDelete("api/evaluaciones/{evaluacionId:guid}/preguntas/{preguntaId:guid}/opciones/{opcionId:guid}")]
    public async Task<IActionResult> EliminarOpcion(
        Guid evaluacionId, Guid preguntaId, Guid opcionId,
        CancellationToken cancellationToken)
    {
        await sender.Send(new EliminarOpcionCommand(evaluacionId, preguntaId, opcionId), cancellationToken);
        return NoContent();
    }

    /// <summary>Obtiene preguntas del banco con filtros (para seleccionar masivamente).</summary>
    [HttpGet("api/evaluaciones/{evaluacionId:guid}/preguntas/banco")]
    public async Task<IActionResult> ObtenerPreguntasBanco(
        Guid evaluacionId,
        [FromQuery] string? categoriaIds = null,
        [FromQuery] string? dificultades = null,
        [FromQuery] string? tipoPregunta = null,
        CancellationToken cancellationToken = default)
    {
        var catIds = string.IsNullOrEmpty(categoriaIds) ? null :
            categoriaIds.Split(',').Select(Guid.Parse).ToList();
        var difs = string.IsNullOrEmpty(dificultades) ? null :
            dificultades.Split(',').ToList();

        var query = new ObtenerPreguntasBancoQuery(catIds, difs, tipoPregunta, evaluacionId);
        var resultado = await sender.Send(query, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>Agrega preguntas del banco (clonadas) a una evaluación.</summary>
    [HttpPost("api/evaluaciones/{evaluacionId:guid}/preguntas/agregar-del-banco")]
    public async Task<IActionResult> AgregarDelBanco(
        Guid evaluacionId,
        [FromBody] AgregarDelBancoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AgregarPreguntasMasivasCommand(evaluacionId, request.PreguntaIds);
        var resultado = await sender.Send(command, cancellationToken);
        return Ok(resultado);
    }
}

public sealed record AgregarPreguntaRequest(
    string Texto, TipoPregunta TipoPregunta, NivelDificultad NivelDificultad,
    int? LimiteTiempoSegundos, bool PermiteAdjunto, bool EsRevisionManual,
    Guid? CategoriaId = null);

public sealed record ActualizarPreguntaRequest(
    string Texto, TipoPregunta TipoPregunta, NivelDificultad NivelDificultad,
    int? LimiteTiempoSegundos, bool PermiteAdjunto, bool EsRevisionManual,
    Guid? CategoriaId = null);

public sealed record AgregarOpcionRequest(string Texto, int? Puntuacion, bool EsRevisionManual);

public sealed record ActualizarOpcionRequest(string Texto, int? Puntuacion, bool EsRevisionManual);

public sealed record AgregarDelBancoRequest(List<Guid> PreguntaIds);
