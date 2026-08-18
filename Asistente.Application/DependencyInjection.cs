using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asistente.Application.Interfaces;
using Asistente.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Asistente.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IEnviarMensajeService, EnviarMensajeService>();

        return services;
    }
}