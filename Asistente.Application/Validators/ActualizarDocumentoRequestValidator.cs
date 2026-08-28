// ActualizarDocumentoRequestValidator.cs
using Asistente.Application.DTOs;
using FluentValidation;

namespace Asistente.Application.Validators;

public class ActualizarDocumentoRequestValidator
    : AbstractValidator<ActualizarDocumentoRequestDto>
{
    public ActualizarDocumentoRequestValidator()
    {
        RuleFor(request => request.Nombre)
            .NotEmpty()
            .WithMessage("El nombre del documento es obligatorio.")
            .MaximumLength(250)
            .WithMessage("El nombre no puede superar los 250 caracteres.");

        RuleFor(request => request.Descripcion)
            .MaximumLength(1000)
            .WithMessage("La descripción no puede superar los 1000 caracteres.");

        RuleFor(request => request.IdCategoria)
            .GreaterThan(0)
            .WithMessage("La categoría del documento es obligatoria.");
    }
}