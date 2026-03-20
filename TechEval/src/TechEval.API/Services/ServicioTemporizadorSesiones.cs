using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TechEval.API.Hubs;
using TechEval.Domain.Sesiones;

namespace TechEval.API.Services;

/// <summary>
/// Servicio de fondo que gestiona temporizadores para sesiones de evaluación.
/// </summary>
public sealed class ServicioTemporizadorSesiones : BackgroundService
{
    private readonly IHubContext<HubEvaluacion> _hubContext;
    private readonly ILogger<ServicioTemporizadorSesiones> _logger;
    private readonly Dictionary<Guid, TemporizadorSesion> _temporizadores = new();
    private readonly object _lockObj = new();

    public ServicioTemporizadorSesiones(
        IHubContext<HubEvaluacion> hubContext,
        ILogger<ServicioTemporizadorSesiones> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ServicioTemporizadorSesiones iniciado");

        // Recargar sesiones activas cada 10 segundos
        using var timerRecarga = new PeriodicTimer(TimeSpan.FromSeconds(10));

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Recarga de sesiones activas
                await RecargarSesionesActivas();

                // Tick del temporizador
                await TickTemporizadores();

                // Esperar 1 segundo antes del siguiente tick
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("ServicioTemporizadorSesiones cancelado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ServicioTemporizadorSesiones");
            throw;
        }
    }

    private async Task RecargarSesionesActivas()
    {
        try
        {
            // Obtener sesiones en progreso (esto podría optimizarse con una query específica)
            // Por ahora, simplemente monitoreamos las sesiones que ya están registradas
            
            lock (_lockObj)
            {
                // Limpiar sesiones completadas
                var completadas = _temporizadores
                    .Where(kvp => kvp.Value.Estado == EstadoSesion.Completada || 
                                 kvp.Value.Estado == EstadoSesion.Abandonada)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var id in completadas)
                {
                    _temporizadores.Remove(id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recargando sesiones activas");
        }
    }

    private async Task TickTemporizadores()
    {
        lock (_lockObj)
        {
            foreach (var (sesionId, temp) in _temporizadores.ToList())
            {
                if (temp.Estado != EstadoSesion.EnProgreso)
                    continue;

                temp.SegundosRestantes--;

                // Emitir tick cada segundo
                var grupoSesion = $"sesion_{sesionId:N}";
                _ = _hubContext.Clients
                    .Group(grupoSesion)
                    .SendAsync("TickTemporizador", new
                    {
                        sesionId,
                        segundosRestantes = temp.SegundosRestantes
                    });

                // Si se agota el tiempo
                if (temp.SegundosRestantes <= 0)
                {
                    temp.Estado = EstadoSesion.Abandonada;
                    _ = _hubContext.Clients
                        .Group(grupoSesion)
                        .SendAsync("TiempoSesionAgotado", new { sesionId });
                    
                    _logger.LogInformation("Sesión {SesionId} expirada por timeout", sesionId);
                }
            }
        }
    }

    public void RegistrarSesion(Guid sesionId, int minutosMaximos, EstadoSesion estado)
    {
        lock (_lockObj)
        {
            _temporizadores[sesionId] = new TemporizadorSesion
            {
                SesionId = sesionId,
                SegundosRestantes = minutosMaximos * 60,
                Estado = estado,
                RegistradaEn = DateTime.UtcNow
            };

            _logger.LogInformation(
                "Sesión registrada en temporizador: {SesionId}, {MinutosMaximos} minutos",
                sesionId,
                minutosMaximos
            );
        }
    }

    public void ActualizarEstado(Guid sesionId, EstadoSesion nuevoEstado)
    {
        lock (_lockObj)
        {
            if (_temporizadores.TryGetValue(sesionId, out var temp))
            {
                temp.Estado = nuevoEstado;
            }
        }
    }

    public int ObtenerSegundosRestantes(Guid sesionId)
    {
        lock (_lockObj)
        {
            return _temporizadores.TryGetValue(sesionId, out var temp)
                ? temp.SegundosRestantes
                : 0;
        }
    }
}

/// <summary>
/// Modelo interno para tracking de temporizadores.
/// </summary>
internal sealed class TemporizadorSesion
{
    public Guid SesionId { get; set; }
    public int SegundosRestantes { get; set; }
    public EstadoSesion Estado { get; set; }
    public DateTime RegistradaEn { get; set; }
}
