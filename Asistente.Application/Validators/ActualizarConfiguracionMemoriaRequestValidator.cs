using Asistente.Application.DTOs;
using FluentValidation;

namespace Asistente.Application.Validators;

public class ActualizarConfiguracionMemoriaRequestValidator
    : AbstractValidator<ActualizarConfiguracionMemoriaRequestDto>
{
    public ActualizarConfiguracionMemoriaRequestValidator()
    {
        RuleFor(x => x.MaximoMensajesContexto)
            .InclusiveBetween(2, 50)
            .WithMessage(
                "El máximo de mensajes de contexto debe estar entre 2 y 50.");

        RuleFor(x => x.MaximoTokensContexto)
            .InclusiveBetween(256, 16000)
            .WithMessage(
                "El máximo de tokens de contexto debe estar entre 256 y 16000.");

        RuleFor(x => x.LongitudResumen)
            .InclusiveBetween(100, 8000)
            .WithMessage(
                "La longitud del resumen debe estar entre 100 y 8000 caracteres.");

        RuleFor(x => x.CantidadConversacionesVisibles)
            .InclusiveBetween(1, 100)
            .WithMessage(
                "La cantidad de conversaciones visibles debe estar entre 1 y 100.");
    }
}