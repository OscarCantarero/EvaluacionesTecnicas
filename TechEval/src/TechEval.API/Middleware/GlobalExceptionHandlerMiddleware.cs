using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Common.Errors;

namespace TechEval.API.Middleware;

public sealed class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error no controlado: {Mensaje}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, titulo, detalle) = exception switch
        {
            ValidationException ve => (
                HttpStatusCode.BadRequest,
                "Error de validación",
                (object)new { Errores = ve.Errores.Select(e => new { e.Propiedad, e.Mensaje }) }),

            NotFoundException => (HttpStatusCode.NotFound, "Recurso no encontrado", (object)exception.Message),
            ConflictException => (HttpStatusCode.Conflict, "Conflicto", (object)exception.Message),
            DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "Conflicto de concurrencia",
                (object)"El recurso fue modificado por otro proceso. Recarga e intenta nuevamente."),
            UnauthorizedException => (HttpStatusCode.Unauthorized, "No autorizado", (object)exception.Message),
            InvalidOperationException => (HttpStatusCode.UnprocessableEntity, "Operación no permitida", (object)exception.Message),
            DomainException de => (HttpStatusCode.UnprocessableEntity, "Regla de negocio violada",
                (object)new { de.Error.Codigo, de.Error.Descripcion }),

            _ => (HttpStatusCode.InternalServerError, "Error interno del servidor", (object)"Ha ocurrido un error inesperado.")
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var respuesta = new
        {
            Titulo = titulo,
            Status = (int)statusCode,
            Detalle = detalle
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(respuesta,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
