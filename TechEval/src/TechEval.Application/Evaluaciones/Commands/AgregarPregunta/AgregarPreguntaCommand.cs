using MediatR;
using TechEval.Domain.Common.ValueObjects;

namespace TechEval.Application.Evaluaciones.Commands.AgregarPregunta;

public sealed record AgregarPreguntaCommand(
    Guid EvaluacionId,
    string Texto,
    TipoPregunta TipoPregunta,
    NivelDificultad NivelDificultad,
    int? LimiteTiempoSegundos,
    bool PermiteAdjunto,
    bool EsRevisionManual,
    Guid? CategoriaId = null
) : IRequest<Guid>;
