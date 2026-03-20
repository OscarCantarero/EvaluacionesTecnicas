using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechEval.Application.Evaluaciones.Commands.ActualizarEvaluacion;
using TechEval.Application.Evaluaciones.Commands.ActivarEvaluacion;
using TechEval.Application.Evaluaciones.Commands.CrearEvaluacion;
using TechEval.Application.Evaluaciones.Commands.EliminarEvaluacion;
using TechEval.Application.Evaluaciones.Queries.ListarEvaluaciones;
using TechEval.Application.Evaluaciones.Queries.ObtenerEvaluacion;
using TechEval.Application.Resultados.Queries.CompararCandidatos;
using TechEval.Application.Resultados.Queries.ObtenerRanking;

namespace TechEval.API.Controllers;

[ApiController]
[Route("api/evaluaciones")]
[Authorize(Roles = "Evaluador,Administrador")]
public sealed class EvaluacionesController(ISender sender) : ControllerBase
{
    /// <summary>Lista evaluaciones con paginación y filtros. Pasar soloMias=true para filtrar por evaluador autenticado.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] bool soloMias = false,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10,
        [FromQuery] string? busqueda = null,
        [FromQuery] string? estado = null,
        CancellationToken cancellationToken = default)
    {
        var resultado = await sender.Send(
            new ListarEvaluacionesQuery(soloMias, pagina, tamanoPagina, busqueda, estado), cancellationToken);
        return Ok(resultado);
    }

    /// <summary>Obtiene una evaluación con todas sus preguntas y opciones.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new ObtenerEvaluacionQuery(id), cancellationToken);
        return Ok(resultado);
    }

    /// <summary>Crea una nueva evaluación.</summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearEvaluacionCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id }, new { Id = id });
    }

    /// <summary>Actualiza nombre, descripción y configuración de orden de una evaluación.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ActualizarEvaluacionRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new ActualizarEvaluacionCommand(id, request.Nombre, request.Descripcion, request.OrdenAleatorio, request.OrdenPorDificultad), cancellationToken);
        return NoContent();
    }

    /// <summary>Elimina una evaluación y todas sus preguntas.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new EliminarEvaluacionCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Activa una evaluación (cambia estado de Borrador a Activa).</summary>
    [HttpPut("{id:guid}/activar")]
    public async Task<IActionResult> Activar(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new ActivarEvaluacionCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Compara múltiples candidatos de la misma evaluación.</summary>
    [HttpPost("{id:guid}/comparar")]
    public async Task<IActionResult> Comparar(Guid id, [FromBody] CompararCandidatosRequest request, CancellationToken cancellationToken)
    {
        var query = new CompararCandidatosQuery(id, request.SesionIds);
        var resultado = await sender.Send(query, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>Obtiene ranking de candidatos por puntaje total para una evaluación.</summary>
    [HttpGet("{id:guid}/ranking")]
    public async Task<IActionResult> Ranking(Guid id, CancellationToken cancellationToken)
    {
        var query = new ObtenerRankingQuery(id);
        var resultado = await sender.Send(query, cancellationToken);
        return Ok(resultado);
    }
}

public sealed record ActualizarEvaluacionRequest(string Nombre, string? Descripcion, bool OrdenAleatorio, bool OrdenPorDificultad);
public sealed record CompararCandidatosRequest(List<Guid> SesionIds);
