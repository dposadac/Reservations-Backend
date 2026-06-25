using Ceiba.LiveEvent.Reservations.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Ceiba.LiveEvent.Reservations.Api.IntegrationTests;

/// <summary>
/// Levanta la API en memoria apuntando a un contenedor PostgreSQL efímero
/// (Testcontainers). El esquema se crea ejecutando el script Database First
/// <c>001_create_todo_items.sql</c>, sin migraciones de EF Core.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
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

        var scriptPath = Path.Combine(AppContext.BaseDirectory, "db", "07_create_table_todo_items.sql");
        var script = await File.ReadAllTextAsync(scriptPath);

        await using var connection = new NpgsqlConnection(_container.GetConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(script, connection);
        await command.ExecuteNonQueryAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _container.DisposeAsync();
        await base.DisposeAsync();
    }
}
