using Asistente.Application.DTOs;
using FluentValidation;

namespace Asistente.Application.Validators;

public class CrearPromptSistemaRequestValidator
    : AbstractValidator<CrearPromptSistemaRequestDto>
{
    public CrearPromptSistemaRequestValidator()
    {
        RuleFor(request => request.Nombre)
            .NotEmpty()
            .MaximumLength(150)
            .WithMessage("El nombre del prompt es obligatorio y admite hasta 150 caracteres.");

        RuleFor(request => request.Contenido)
            .NotEmpty()
            .MaximumLength(12000)
            .WithMessage("El contenido del prompt es obligatorio y admite hasta 12000 caracteres.");
    }
}