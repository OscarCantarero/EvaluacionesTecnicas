using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechEval.Application.Categorias.Commands.ActualizarCategoria;
using TechEval.Application.Categorias.Commands.CrearCategoria;
using TechEval.Application.Categorias.Commands.EliminarCategoria;
using TechEval.Application.Categorias.Queries.ListarCategorias;

namespace TechEval.API.Controllers;

[ApiController]
[Route("api/categorias")]
[Authorize(Roles = "Evaluador,Administrador")]
public sealed class CategoriasController(ISender sender) : ControllerBase
{
    /// <summary>Lista todas las categorías disponibles.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new ListarCategoriasQuery(), cancellationToken);
        return Ok(resultado);
    }

    /// <summary>Crea una nueva categoría.</summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearCategoriaRequest request, CancellationToken cancellationToken)
    {
        var id = await sender.Send(new CrearCategoriaCommand(request.Nombre, request.Descripcion), cancellationToken);
        return Created($"api/categorias/{id}", new { Id = id });
    }

    /// <summary>Actualiza una categoría existente.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ActualizarCategoriaRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new ActualizarCategoriaCommand(id, request.Nombre, request.Descripcion), cancellationToken);
        return NoContent();
    }

    /// <summary>Elimina una categoría (las preguntas asociadas quedan sin categoría).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new EliminarCategoriaCommand(id), cancellationToken);
        return NoContent();
    }
}

public sealed record CrearCategoriaRequest(string Nombre, string? Descripcion);
public sealed record ActualizarCategoriaRequest(string Nombre, string? Descripcion);
