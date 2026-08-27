using Asistente.Application.DTOs;
using FluentValidation;

namespace Asistente.Application.Validators;

public class RenombrarConversacionRequestValidator
    : AbstractValidator<RenombrarConversacionRequestDto>
{
    public RenombrarConversacionRequestValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty()
            .WithMessage("El título de la conversación es obligatorio.")
            .MaximumLength(200)
            .WithMessage(
                "El título de la conversación no puede superar 200 caracteres.");
    }
}