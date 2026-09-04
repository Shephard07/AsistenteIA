using Asistente.Application.DTOs;
using FluentValidation;

namespace Asistente.Application.Validators;

public class ActualizarConfiguracionRagRequestValidator
    : AbstractValidator<ActualizarConfiguracionRagRequestDto>
{
    public ActualizarConfiguracionRagRequestValidator()
    {
        RuleFor(x => x.ModeloEmbeddings)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage(
                "El modelo de embeddings es obligatorio y debe tener " +
                "como máximo 100 caracteres.");

        RuleFor(x => x.CantidadResultados)
            .InclusiveBetween(1, 20)
            .WithMessage(
                "La cantidad de resultados debe estar entre 1 y 20.");

        RuleFor(x => x.PuntajeMinimo)
            .InclusiveBetween(0m, 1m)
            .WithMessage(
                "El puntaje mínimo debe estar entre 0 y 1.");

        RuleFor(x => x.LongitudMaximaContexto)
            .InclusiveBetween(500, 16000)
            .WithMessage(
                "La longitud máxima del contexto debe estar entre " +
                "500 y 16000 caracteres.");
    }
}