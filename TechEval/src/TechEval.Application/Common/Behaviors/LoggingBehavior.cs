using MediatR;
using Microsoft.Extensions.Logging;

namespace TechEval.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var nombre = typeof(TRequest).Name;
        logger.LogInformation("Ejecutando {Nombre}", nombre);

        try
        {
            var resultado = await next(cancellationToken);
            logger.LogInformation("{Nombre} completado exitosamente", nombre);
            return resultado;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al ejecutar {Nombre}", nombre);
            throw;
        }
    }
}
