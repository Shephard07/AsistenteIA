using Asistente.Application.Interfaces;
using Asistente.Application.Services;
using Asistente.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Asistente.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IEnviarMensajeService, EnviarMensajeService>();

        services.AddValidatorsFromAssemblyContaining<
            EnviarMensajeRequestValidator>();

        return services;
    }
}