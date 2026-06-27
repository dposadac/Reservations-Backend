using Ceiba.LiveEvent.Reservations.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Ceiba.LiveEvent.Reservations.Api.IntegrationTests;

/// <summary>
/// Levanta la API en memoria apuntando a un contenedor PostgreSQL efímero
/// (Testcontainers). El esquema se crea ejecutando los scripts Database First de
/// <c>db/scripts</c> (omitiendo el 00, que crea la base), sin migraciones de EF Core.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>Lugar determinista sembrado para las pruebas que crean eventos.</summary>
    public static readonly Guid TestVenueId = new("aaaaaaaa-0000-0000-0000-000000000001");

    public const int TestVenueCapacity = 500;

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("ceiba_reservations")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Sobrescribe la cadena de conexión que consume AddInfrastructure.
        builder.UseSetting(
            $"ConnectionStrings:{DependencyInjection.ConnectionStringName}",
            _container.GetConnectionString());
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _container.StartAsync();

        await using var connection = new NpgsqlConnection(_container.GetConnectionString());
        await connection.OpenAsync();

        await ApplySchemaScriptsAsync(connection);
        await SeedTestVenueAsync(connection);
    }

    private static async Task ApplySchemaScriptsAsync(NpgsqlConnection connection)
    {
        var scriptsDirectory = Path.Combine(AppContext.BaseDirectory, "db");

        // Orden numérico ascendente; se omite 00_create_database (la base ya existe).
        var scripts = Directory.GetFiles(scriptsDirectory, "*.sql")
            .Where(path => !Path.GetFileName(path).StartsWith("00", StringComparison.Ordinal))
            .OrderBy(Path.GetFileName, StringComparer.Ordinal);

        foreach (var script in scripts)
        {
            var sql = await File.ReadAllTextAsync(script);
            await using var command = new NpgsqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }
    }

    private static async Task SeedTestVenueAsync(NpgsqlConnection connection)
    {
        const string sql = """
            INSERT INTO public.venue (id, venue_name, quantity, city)
            VALUES (@id, 'Auditorio de Pruebas', @capacity, 'Bogotá')
            ON CONFLICT (id) DO NOTHING;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", TestVenueId);
        command.Parameters.AddWithValue("capacity", TestVenueCapacity);
        await command.ExecuteNonQueryAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _container.DisposeAsync();
        await base.DisposeAsync();
    }
}
