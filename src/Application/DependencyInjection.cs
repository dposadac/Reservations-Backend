using System.Reflection;
using Ceiba.LiveEvent.Reservations.Application.Common.Behaviours;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Ceiba.LiveEvent.Reservations.Application;

/// <summary>
/// Registro de los servicios de la capa de aplicación: MediatR, validadores de
/// FluentValidation y los comportamientos del pipeline.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
