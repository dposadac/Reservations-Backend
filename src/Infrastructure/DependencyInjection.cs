using Ceiba.LiveEvent.Reservations.Application.Common.Interfaces;
using Ceiba.LiveEvent.Reservations.Infrastructure.Common;
using Ceiba.LiveEvent.Reservations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ceiba.LiveEvent.Reservations.Infrastructure;

/// <summary>Registro de los servicios de infraestructura (EF Core + PostgreSQL).</summary>
public static class DependencyInjection
{
    public const string ConnectionStringName = "Postgres";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"No se encontró la cadena de conexión '{ConnectionStringName}'.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddScoped<ITodoRepository, EfTodoRepository>();
        services.AddScoped<IVenueRepository, EfVenueRepository>();
        services.AddScoped<IEventTypeRepository, EfEventTypeRepository>();
        services.AddScoped<IEventStatusRepository, EfEventStatusRepository>();
        services.AddScoped<IReservationStatusRepository, EfReservationStatusRepository>();
        services.AddScoped<IEventRepository, EfEventRepository>();
        services.AddScoped<IReservationRepository, EfReservationRepository>();

        return services;
    }
}
