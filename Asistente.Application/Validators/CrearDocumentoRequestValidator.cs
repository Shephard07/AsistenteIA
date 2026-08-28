// CrearDocumentoRequestValidator.cs
using Asistente.Application.DTOs;
using FluentValidation;

namespace Asistente.Application.Validators;

public class CrearDocumentoRequestValidator
    : AbstractValidator<CrearDocumentoRequestDto>
{
    public CrearDocumentoRequestValidator()
    {
        RuleFor(request => request.Codigo)
            .NotEmpty()
            .WithMessage("El código del documento es obligatorio.")
            .MaximumLength(50)
            .WithMessage("El código no puede superar los 50 caracteres.")
            .Matches("^[A-Za-z0-9_-]+$")
            .WithMessage(
                "El código solo puede contener letras, números, guiones y guiones bajos.");

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