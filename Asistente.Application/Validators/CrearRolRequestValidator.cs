using Asistente.Application.DTOs;
using FluentValidation;

namespace Asistente.Application.Validators;

public class CrearRolRequestValidator
    : AbstractValidator<CrearRolRequestDto>
{
    public CrearRolRequestValidator()
    {
        RuleFor(request => request.Nombre)
            .NotEmpty()
                .WithMessage("El nombre del rol es obligatorio.")
            .MaximumLength(50)
                .WithMessage("El nombre del rol admite como máximo 50 caracteres.");

        RuleFor(request => request.Descripcion)
            .NotEmpty()
                .WithMessage("La descripción del rol es obligatoria.")
            .MaximumLength(250)
                .WithMessage("La descripción admite como máximo 250 caracteres.");
    }
}