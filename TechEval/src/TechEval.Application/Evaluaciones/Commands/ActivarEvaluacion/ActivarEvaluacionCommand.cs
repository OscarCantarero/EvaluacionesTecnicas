using MediatR;

namespace TechEval.Application.Evaluaciones.Commands.ActivarEvaluacion;

public sealed record ActivarEvaluacionCommand(Guid Id) : IRequest;
