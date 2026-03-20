using TechEval.Domain.Common.Abstractions;

namespace TechEval.Domain.Evaluaciones.Events;

public sealed record EvaluacionCreadaEvent(Guid EvaluacionId, string Nombre) : IDomainEvent;
