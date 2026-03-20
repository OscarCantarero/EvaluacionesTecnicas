using MediatR;

namespace TechEval.Application.Evaluaciones.Commands.CrearEvaluacion;

public sealed record CrearEvaluacionCommand(
    string Nombre,
    string? Descripcion,
    bool OrdenAleatorio,
    bool OrdenPorDificultad
) : IRequest<Guid>;
