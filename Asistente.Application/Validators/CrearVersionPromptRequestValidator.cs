using Asistente.Application.DTOs;
using FluentValidation;

namespace Asistente.Application.Validators;

public class CrearVersionPromptRequestValidator
    : AbstractValidator<CrearVersionPromptRequestDto>
{
    public CrearVersionPromptRequestValidator()
    {
        RuleFor(request => request.Nombre)
            .NotEmpty()
            .MaximumLength(150)
            .WithMessage("El nombre del prompt es obligatorio y admite hasta 150 caracteres.");

        RuleFor(request => request.Contenido)
            .NotEmpty()
            .MaximumLength(12000)
            .WithMessage("El contenido del prompt es obligatorio y admite hasta 12000 caracteres.");

        RuleFor(request => request.MotivoCambio)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("El motivo del cambio es obligatorio y admite hasta 500 caracteres.");
    }
}