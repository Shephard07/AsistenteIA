using Asistente.Application.DTOs;
using FluentValidation;

namespace Asistente.Application.Validators;

public class ActualizarUsuarioRequestValidator
    : AbstractValidator<ActualizarUsuarioRequestDto>
{
    public ActualizarUsuarioRequestValidator()
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
    }
}