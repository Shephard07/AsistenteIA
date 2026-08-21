using Asistente.Application.DTOs;
using FluentValidation;

namespace Asistente.Application.Validators;

public class CrearUsuarioRequestValidator
    : AbstractValidator<CrearUsuarioRequestDto>
{
    public CrearUsuarioRequestValidator()
    {
        RuleFor(request => request.Usuario)
            .NotEmpty().WithMessage("El usuario es obligatorio.")
            .MaximumLength(50)
            .WithMessage("El usuario admite como máximo 50 caracteres.");

        RuleFor(request => request.Nombres)
            .NotEmpty().WithMessage("Los nombres son obligatorios.")
            .MaximumLength(100)
            .WithMessage("Los nombres admiten como máximo 100 caracteres.");

        RuleFor(request => request.Apellidos)
            .NotEmpty().WithMessage("Los apellidos son obligatorios.")
            .MaximumLength(100)
            .WithMessage("Los apellidos admiten como máximo 100 caracteres.");

        RuleFor(request => request.Correo)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("El correo no tiene un formato válido.")
            .MaximumLength(150)
            .WithMessage("El correo admite como máximo 150 caracteres.");

        RuleFor(request => request.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8)
            .WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .MaximumLength(100)
            .WithMessage("La contraseña admite como máximo 100 caracteres.")
            .Matches("[A-Z]")
            .WithMessage("La contraseña debe incluir una mayúscula.")
            .Matches("[a-z]")
            .WithMessage("La contraseña debe incluir una minúscula.")
            .Matches("[0-9]")
            .WithMessage("La contraseña debe incluir un número.")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("La contraseña debe incluir un carácter especial.");

        RuleFor(request => request.IdsRoles)
            .NotEmpty()
            .WithMessage("Debe asignar al menos un rol al usuario.");

        RuleForEach(request => request.IdsRoles)
            .GreaterThan(0)
            .WithMessage("El identificador de rol no es válido.");
    }
}