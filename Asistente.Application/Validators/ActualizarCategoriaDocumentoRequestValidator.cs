// ActualizarCategoriaDocumentoRequestValidator.cs
using Asistente.Application.DTOs;
using FluentValidation;

namespace Asistente.Application.Validators;

public class ActualizarCategoriaDocumentoRequestValidator
    : AbstractValidator<ActualizarCategoriaDocumentoRequestDto>
{
    public ActualizarCategoriaDocumentoRequestValidator()
    {
        RuleFor(request => request.Nombre)
            .NotEmpty()
            .WithMessage("El nombre de la categoría es obligatorio.")
            .MaximumLength(100)
            .WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(request => request.Descripcion)
            .MaximumLength(500)
            .WithMessage("La descripción no puede superar los 500 caracteres.");
    }
}