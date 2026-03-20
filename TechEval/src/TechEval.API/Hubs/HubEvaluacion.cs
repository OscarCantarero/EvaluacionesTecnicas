using Microsoft.AspNetCore.SignalR;
using TechEval.Application.Common.Interfaces;
using TechEval.Application.Sesiones.Commands.RegistrarRespuesta;
using TechEval.Application.Sesiones.Queries.ObtenerSesion;
using MediatR;

namespace TechEval.API.Hubs;

/// <summary>
/// Hub de SignalR para sesiones en vivo. Gestiona comunicación real-time con candidatos.
/// 
/// Eventos emitidos al cliente:
/// - PreguntaCargada: Nueva pregunta disponible
/// - TickTemporizador: Actualización del contador regresivo
/// - SiguientePregunta: Avanzar a la siguiente pregunta
/// - SesionCompletada: Sesión finalizada
/// 
/// Métodos cliente puede llamar:
/// - IniciarSesion: Candidato comienza
/// - ResponderPregunta: Enviar respuesta
/// - TabViolation: Candidato cambió de pestaña
/// </summary>
public sealed class HubEvaluacion : Hub
{
    private readonly IMediator _mediator;
    private readonly IContextoUsuario _contextoUsuario;

    public HubEvaluacion(IMediator mediator, IContextoUsuario contextoUsuario)
    {
        _mediator = mediator;
        _contextoUsuario = contextoUsuario;
    }

    /// <summary>
    /// Se llama cuando el cliente se conecta. Obtiene el sesionId del query parameter.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var sesionId = httpContext?.Request.Query["sesionId"].ToString();

        if (string.IsNullOrEmpty(sesionId) || !Guid.TryParse(sesionId, out var sid))
        {
            Context.Abort();
            return;
        }

        // Agregar cliente a grupo de sesión (ej: "sesion_550e8400...")
        var grupoSesion = $"sesion_{sid:N}";
        await Groups.AddToGroupAsync(Context.ConnectionId, grupoSesion);
    }

    /// <summary>
    /// Candidato envía respuesta a una pregunta
    /// </summary>
    public async Task ResponderPregunta(ResponderPreguntaRequest request)
    {
        try
        {
            var command = new RegistrarRespuestaCommand(
                SesionId: request.SesionId,
                PreguntaId: request.PreguntaId,
                Texto: request.Texto,
                TiempoEmpleadoSegundos: request.TiempoEmpleadoSegundos,
                FueExpirado: request.FueExpirado
            );

            var response = await _mediator.Send(command);

            // Emitir al cliente mismo el resultado
            await Clients.Caller.SendAsync(
                "RespuestaRegistrada",
                new
                {
                    success = true,
                    sesionCompletada = response.SesionCompletada,
                    preguntaSiguiente = response.PreguntaSiguiente,
                    progreso = new { respondidas = response.PreguntasRespondidas, total = response.TotalPreguntas }
                }
            );
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Candidato reporta cambio de pestaña
    /// </summary>
    public async Task ReportarCambiosPestana(Guid sesionId)
    {
        try
        {
            // Registrar violación en base de datos
            // (En una implementación completa, usaría un command para esto)
            
            var grupoSesion = $"sesion_{sesionId:N}";
            
            // Notificar a todo el grupo sobre la violación
            await Clients.Group(grupoSesion).SendAsync(
                "ViolacionDetectada",
                new { sesionId, timestamp = DateTime.UtcNow }
            );
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Temporizador agotado para una pregunta
    /// </summary>
    public async Task TiempoAgotado(Guid sesionId, Guid preguntaId)
    {
        try
        {
            var grupoSesion = $"sesion_{sesionId:N}";
            
            // Notificar que se agotó el tiempo
            await Clients.Group(grupoSesion).SendAsync(
                "TiempoAgotadoPregunta",
                new { preguntaId, timestamp = DateTime.UtcNow }
            );
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", new { mensaje = ex.Message });
        }
    }
}

public sealed record ResponderPreguntaRequest(
    Guid SesionId,
    Guid PreguntaId,
    string Texto,
    int TiempoEmpleadoSegundos,
    bool FueExpirado
);
