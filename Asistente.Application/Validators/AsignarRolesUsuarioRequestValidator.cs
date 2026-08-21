using Asistente.Application.DTOs;
using FluentValidation;

namespace Asistente.Application.Validators;

public class AsignarRolesUsuarioRequestValidator
    : AbstractValidator<AsignarRolesUsuarioRequestDto>
{
    public AsignarRolesUsuarioRequestValidator()
    {
        RuleFor(request => request.IdsRoles)
            .NotEmpty()
            .WithMessage("Debe asignar al menos un rol al usuario.");

        RuleForEach(request => request.IdsRoles)
            .GreaterThan(0)
            .WithMessage("El identificador de rol no es válido.");
    }
}