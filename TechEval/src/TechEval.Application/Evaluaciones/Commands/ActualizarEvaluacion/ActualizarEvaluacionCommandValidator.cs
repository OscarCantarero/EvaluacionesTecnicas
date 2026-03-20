using FluentValidation;

namespace TechEval.Application.Evaluaciones.Commands.ActualizarEvaluacion;

public sealed class ActualizarEvaluacionCommandValidator : AbstractValidator<ActualizarEvaluacionCommand>
{
    public ActualizarEvaluacionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200);
        RuleFor(x => x.Descripcion)
            .MaximumLength(1000).When(x => x.Descripcion is not null);
        RuleFor(x => x)
            .Must(x => !(x.OrdenAleatorio && x.OrdenPorDificultad))
            .WithMessage("No se puede activar orden aleatorio y progresivo al mismo tiempo.")
            .WithName("Orden");
    }
}
