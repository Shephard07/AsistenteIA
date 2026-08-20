using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asistente.Application.DTOs;
using FluentValidation;

namespace Asistente.Application.Validators;

public sealed class EnviarMensajeRequestValidator
    : AbstractValidator<EnviarMensajeRequestDto>
{
    public EnviarMensajeRequestValidator()
    {
        RuleFor(request => request.Mensaje)
            .NotEmpty()
            .WithMessage("El mensaje es obligatorio.")
            .MaximumLength(2000)
            .WithMessage("El mensaje no puede superar los 2000 caracteres.");

        RuleFor(request => request.IdConversacion)
            .GreaterThan(0)
            .When(request => request.IdConversacion.HasValue)
            .WithMessage(
                "El identificador de conversación debe ser mayor que cero.");
    }
}