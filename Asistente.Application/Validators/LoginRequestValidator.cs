using Asistente.Application.DTOs;
using FluentValidation;

namespace Asistente.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Usuario)
            .NotEmpty()
                .WithMessage("El usuario es obligatorio.")
            .MaximumLength(50)
                .WithMessage("El usuario admite como máximo 50 caracteres.");

        RuleFor(request => request.Password)
            .NotEmpty()
                .WithMessage("La contraseña es obligatoria.")
            .MaximumLength(100)
                .WithMessage("La contraseña admite como máximo 100 caracteres.");
    }
}