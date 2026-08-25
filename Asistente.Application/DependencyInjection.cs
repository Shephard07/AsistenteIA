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
        services.AddScoped<IConversacionService, ConversacionService>();
        services.AddScoped<IMensajeService, MensajeService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IRolService, RolService>();
        services.AddScoped<IAsistenteService, AsistenteService>();
        services.AddScoped<IPromptSistemaService, PromptSistemaService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IAuditoriaService, AuditoriaService>();
        services.AddScoped<IPromptBuilder, PromptBuilder>();
        services.AddScoped<IPruebaPromptService, PruebaPromptService>();

        services.AddValidatorsFromAssemblyContaining<
            EnviarMensajeRequestValidator>();

        return services;
    }
}