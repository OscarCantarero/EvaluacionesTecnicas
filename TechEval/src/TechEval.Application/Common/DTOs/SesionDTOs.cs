namespace TechEval.Application.Common.DTOs;

public sealed record OpcionPreguntaDto(Guid Id, string Texto);

public sealed record PreguntaSesionDto(
    Guid PreguntaId,
    string Texto,
    int Orden,
    int? LimiteTiempoSegundos,
    string TipoPregunta,
    List<OpcionPreguntaDto> Opciones
);

public sealed record RespuestaSesionDto(
    Guid PreguntaId,
    string Texto,
    int TiempoEmpleadoSegundos,
    bool FueExpirado
);
