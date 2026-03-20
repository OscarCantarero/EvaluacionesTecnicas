using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TechEval.Application.Common.Interfaces;

namespace TechEval.Infrastructure.Services;

public sealed class EvaluacionIAOptions
{
    public const string SectionName = "EvaluacionIA";
    public string EndpointUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 60;
}

public sealed class ServicioEvaluacionIA : IServicioEvaluacionIA
{
    private readonly HttpClient _httpClient;
    private readonly EvaluacionIAOptions _options;
    private readonly ILogger<ServicioEvaluacionIA> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ServicioEvaluacionIA(
        HttpClient httpClient,
        IOptions<EvaluacionIAOptions> options,
        ILogger<ServicioEvaluacionIA> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<RespuestaEvaluacionIA> EvaluarRespuestaAsync(
        SolicitudEvaluacionIA solicitud,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Enviando solicitud de evaluación IA para pregunta {PreguntaId} con escala {Escala}",
            solicitud.PreguntaId, solicitud.EscalaPuntuacion);

        var payload = new
        {
            preguntaId = solicitud.PreguntaId.ToString(),
            textoPregunta = solicitud.TextoPregunta,
            respuestaCandidato = solicitud.RespuestaCandidato,
            contextoEvaluador = solicitud.ContextoEvaluador,
            escalaPuntuacion = solicitud.EscalaPuntuacion,
            rubrica = solicitud.Rubrica ?? string.Empty,
            formatoRespuesta = new
            {
                puntajeSugerido = "decimal entre 0 y escalaPuntuacion",
                justificacion = "texto explicando la calificación",
                aspectosPositivos = "lista de strings con aspectos positivos",
                aspectosNegativos = "lista de strings con áreas de mejora"
            }
        };

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            var response = await _httpClient.PostAsJsonAsync(
                _options.EndpointUrl, payload, JsonOptions, cts.Token);

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync(cts.Token);

            _logger.LogDebug("Respuesta IA raw: {Body}", body);

            var iaResponse = JsonSerializer.Deserialize<IAExternalResponse>(body, JsonOptions);

            if (iaResponse is null)
            {
                _logger.LogWarning("Respuesta IA vacía o inválida, retornando fallback");
                return CrearFallback(solicitud, "Respuesta del servicio IA vacía");
            }

            var puntaje = Math.Clamp(iaResponse.PuntajeSugerido, 0, solicitud.EscalaPuntuacion);

            _logger.LogInformation(
                "Evaluación IA recibida para pregunta {PreguntaId}: {Puntaje}/{Escala}",
                solicitud.PreguntaId, puntaje, solicitud.EscalaPuntuacion);

            return new RespuestaEvaluacionIA(
                PreguntaId: solicitud.PreguntaId,
                PuntajeSugerido: puntaje,
                EscalaPuntuacion: solicitud.EscalaPuntuacion,
                Justificacion: iaResponse.Justificacion ?? "Sin justificación proporcionada",
                RequiereRevisionManual: true,
                AspectosPositivos: iaResponse.AspectosPositivos ?? [],
                AspectosNegativos: iaResponse.AspectosNegativos ?? []
            );
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Timeout al llamar servicio IA ({Timeout}s)", _options.TimeoutSeconds);
            return CrearFallback(solicitud, $"Timeout al contactar servicio de IA ({_options.TimeoutSeconds}s)");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error HTTP al llamar servicio IA");
            return CrearFallback(solicitud, $"Error al contactar servicio de IA: {ex.Message}");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error al deserializar respuesta IA");
            return CrearFallback(solicitud, $"Respuesta del servicio IA con formato inesperado: {ex.Message}");
        }
    }

    private static RespuestaEvaluacionIA CrearFallback(SolicitudEvaluacionIA solicitud, string motivo)
    {
        return new RespuestaEvaluacionIA(
            PreguntaId: solicitud.PreguntaId,
            PuntajeSugerido: 0,
            EscalaPuntuacion: solicitud.EscalaPuntuacion,
            Justificacion: $"[FALLBACK] {motivo}. Asigne puntaje manualmente.",
            RequiereRevisionManual: true,
            AspectosPositivos: [],
            AspectosNegativos: [motivo]
        );
    }
}

internal sealed class IAExternalResponse
{
    [JsonPropertyName("puntajeSugerido")]
    public decimal PuntajeSugerido { get; set; }

    [JsonPropertyName("justificacion")]
    public string? Justificacion { get; set; }

    [JsonPropertyName("aspectosPositivos")]
    public List<string>? AspectosPositivos { get; set; }

    [JsonPropertyName("aspectosNegativos")]
    public List<string>? AspectosNegativos { get; set; }
}
