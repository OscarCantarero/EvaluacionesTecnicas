using MediatR;
using TechEval.Application.Common.DTOs;

namespace TechEval.Application.Sesiones.Queries.ObtenerSesion;

public sealed record ObtenerSesionQuery(
    Guid SesionId
) : IRequest<ObtenerSesionResponse>;

public sealed record ObtenerSesionResponse(
    Guid SesionId,
    Guid EvaluacionId,
    string Estado,
    DateTime CreadaEn,
    DateTime? IniciadaEn,
    DateTime? CompletadaEn,
    int ContadorViolacionesPestana,
    List<PreguntaSesionConRespuestaDto> Preguntas,
    int ProgresoTotal,
    int ProgresoRespondidas
);

public sealed record PreguntaSesionConRespuestaDto(
    Guid Id,
    Guid PreguntaId,
    string Texto,
    int Orden,
    int? LimiteTiempoSegundos,
    RespuestaCandidatoDto? Respuesta,
    bool TiempoExpirado
);

public sealed record RespuestaCandidatoDto(
    string Texto,
    string? UrlAdjunto,
    int TiempoEmpleadoSegundos,
    DateTime BrindadaEn,
    bool FueExpirado
);
