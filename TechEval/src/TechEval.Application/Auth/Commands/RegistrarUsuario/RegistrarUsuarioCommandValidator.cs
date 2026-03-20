using FluentValidation;

namespace TechEval.Application.Auth.Commands.RegistrarUsuario;

public sealed class RegistrarUsuarioCommandValidator : AbstractValidator<RegistrarUsuarioCommand>
{
    private static readonly string[] RolesPermitidos = ["Evaluador", "Candidato", "Administrador"];

    public RegistrarUsuarioCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una mayúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un número.");
        RuleFor(x => x.Rol)
            .NotEmpty()
            .Must(r => RolesPermitidos.Contains(r))
            .WithMessage($"El rol debe ser uno de: {string.Join(", ", RolesPermitidos)}");
    }
}
