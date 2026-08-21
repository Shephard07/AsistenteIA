using Asistente.Application.DTOs;
using FluentValidation;

namespace Asistente.Application.Validators;

public class CambiarPasswordRequestValidator
    : AbstractValidator<CambiarPasswordRequestDto>
{
    public CambiarPasswordRequestValidator()
    {
        RuleFor(request => request.NuevaPassword)
            .NotEmpty().WithMessage("La nueva contraseña es obligatoria.")
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
    }
}