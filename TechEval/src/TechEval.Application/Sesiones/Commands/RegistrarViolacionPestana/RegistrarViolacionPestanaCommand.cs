using MediatR;

namespace TechEval.Application.Sesiones.Commands.RegistrarViolacionPestana;

public sealed record RegistrarViolacionPestanaCommand(
    Guid SesionId
) : IRequest;
