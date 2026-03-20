using MediatR;

namespace TechEval.Application.Evaluaciones.Commands.ActualizarEvaluacion;

public sealed record ActualizarEvaluacionCommand(
    Guid Id,
    string Nombre,
    string? Descripcion,
    bool OrdenAleatorio,
    bool OrdenPorDificultad
) : IRequest;
