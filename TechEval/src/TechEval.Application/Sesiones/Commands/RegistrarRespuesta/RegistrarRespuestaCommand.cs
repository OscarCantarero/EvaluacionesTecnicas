using MediatR;
using TechEval.Application.Common.DTOs;

namespace TechEval.Application.Sesiones.Commands.RegistrarRespuesta;

public sealed record RegistrarRespuestaCommand(
    Guid SesionId,
    Guid PreguntaId,
    string Texto,
    int TiempoEmpleadoSegundos,
    bool FueExpirado = false
) : IRequest<RegistrarRespuestaResponse>;

public sealed record RegistrarRespuestaResponse(
    Guid SesionId,
    PreguntaSesionDto? PreguntaSiguiente,
    int PreguntasRespondidas,
    int TotalPreguntas,
    bool SesionCompletada
);
