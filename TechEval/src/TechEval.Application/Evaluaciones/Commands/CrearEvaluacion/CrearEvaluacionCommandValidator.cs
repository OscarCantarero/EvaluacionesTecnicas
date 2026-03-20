using FluentValidation;

namespace TechEval.Application.Evaluaciones.Commands.CrearEvaluacion;

public sealed class CrearEvaluacionCommandValidator : AbstractValidator<CrearEvaluacionCommand>
{
    public CrearEvaluacionCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(1000).WithMessage("La descripción no puede superar 1000 caracteres.")
            .When(x => x.Descripcion is not null);

        RuleFor(x => x)
            .Must(x => !(x.OrdenAleatorio && x.OrdenPorDificultad))
            .WithMessage("No se puede activar orden aleatorio y progresivo al mismo tiempo.")
            .WithName("Orden");
    }
}
