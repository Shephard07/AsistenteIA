using Asistente.Application.DTOs;
using FluentValidation;

namespace Asistente.Application.Validators;

public abstract class ConfiguracionAsistenteValidatorBase<TRequest>
    : AbstractValidator<TRequest>
    where TRequest : IConfiguracionAsistenteRequestDto
{
    protected ConfiguracionAsistenteValidatorBase()
    {
        RuleFor(request => request.Nombre)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("El nombre del asistente es obligatorio y admite hasta 100 caracteres.");

        RuleFor(request => request.Descripcion)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("La descripción es obligatoria y admite hasta 500 caracteres.");

        RuleFor(request => request.ModeloIA)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("El modelo de IA es obligatorio y admite hasta 100 caracteres.");

        RuleFor(request => request.Idioma)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("El idioma es obligatorio.");

        RuleFor(request => request.LongitudRespuesta)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("La longitud de respuesta es obligatoria.");

        RuleFor(request => request.Formalidad)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("La formalidad es obligatoria.");

        RuleFor(request => request.FormatoRespuesta)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("El formato de respuesta es obligatorio.");

        RuleFor(request => request.Restricciones)
            .NotEmpty()
            .MaximumLength(4000)
            .WithMessage("Las restricciones son obligatorias y admiten hasta 4000 caracteres.");

        RuleFor(request => request.MensajeBienvenida)
            .NotEmpty()
            .MaximumLength(1000)
            .WithMessage("El mensaje de bienvenida es obligatorio y admite hasta 1000 caracteres.");

        RuleFor(request => request.Temperatura)
            .InclusiveBetween(0, 2)
            .WithMessage("La temperatura debe estar entre 0 y 2.");

        RuleFor(request => request.MaxTokens)
            .InclusiveBetween(64, 8192)
            .WithMessage("El máximo de tokens debe estar entre 64 y 8192.");

        RuleFor(request => request.TimeoutSeconds)
            .InclusiveBetween(10, 600)
            .WithMessage("El tiempo máximo de espera debe estar entre 10 y 600 segundos.");
    }
}