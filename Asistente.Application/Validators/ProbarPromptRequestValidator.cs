using Asistente.Application.DTOs;
using FluentValidation;

namespace Asistente.Application.Validators;

public class ProbarPromptRequestValidator
    : AbstractValidator<ProbarPromptRequestDto>
{
    public ProbarPromptRequestValidator()
    {
        RuleFor(request => request.IdAsistente)
            .GreaterThan(0)
            .WithMessage("Debe seleccionar un asistente válido.");

        RuleFor(request => request.IdPrompt)
            .GreaterThan(0)
            .WithMessage("Debe seleccionar una versión de prompt válida.");

        RuleFor(request => request.Mensaje)
            .NotEmpty()
            .MaximumLength(2000)
            .WithMessage("El mensaje de prueba es obligatorio y admite hasta 2000 caracteres.");
    }
}