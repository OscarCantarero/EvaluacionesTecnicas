using FluentValidation;

namespace TechEval.Application.Evaluaciones.Commands.AgregarPregunta;

public sealed class AgregarPreguntaCommandValidator : AbstractValidator<AgregarPreguntaCommand>
{
    public AgregarPreguntaCommandValidator()
    {
        RuleFor(x => x.EvaluacionId).NotEmpty();
        RuleFor(x => x.Texto)
            .NotEmpty().WithMessage("El texto de la pregunta es obligatorio.")
            .MaximumLength(2000);
        RuleFor(x => x.TipoPregunta).IsInEnum();
        RuleFor(x => x.NivelDificultad).IsInEnum();
        RuleFor(x => x.LimiteTiempoSegundos)
            .GreaterThan(0).WithMessage("El límite de tiempo debe ser mayor a cero.")
            .When(x => x.LimiteTiempoSegundos.HasValue);
    }
}
