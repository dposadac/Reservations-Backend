using Ceiba.LiveEvent.Reservations.Application.Events.Commands.CompleteFinishedEvents;
using MediatR;

namespace Ceiba.LiveEvent.Reservations.Api.BackgroundJobs;

/// <summary>
/// Job en segundo plano que ejecuta RN-06 a diario: marca como "completado" los eventos
/// cuya fecha de fin ya pasó. Se ejecuta una vez al arrancar y luego cada 24 horas.
/// </summary>
/// <remarks>
/// En despliegues que escalan a cero (p. ej. Cloud Run) este temporizador no es fiable;
/// en ese caso conviene invocar el endpoint <c>POST /api/event/complete-finished</c> desde
/// un planificador externo (Cloud Scheduler). La lógica es la misma.
/// </remarks>
public sealed class EventCompletionBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromDays(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EventCompletionBackgroundService> _logger;

    public EventCompletionBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<EventCompletionBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        // Se ejecuta en cada tick (cada 24 h), no en el arranque, para no competir con la
        // inicialización de la aplicación. Para una ejecución inmediata, use el endpoint
        // POST /api/event/complete-finished.
        while (await WaitForNextTickAsync(timer, stoppingToken))
        {
            await CompleteFinishedEventsAsync(stoppingToken);
        }
    }

    private async Task CompleteFinishedEventsAsync(CancellationToken cancellationToken)
    {
        try
        {
            // MediatR y el DbContext son scoped; se crea un scope por ejecución.
            using var scope = _scopeFactory.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var count = await sender.Send(new CompleteFinishedEventsCommand(), cancellationToken);
            _logger.LogInformation("RN-06: {Count} evento(s) marcados como completados.", count);
        }
        catch (OperationCanceledException)
        {
            // Apagado normal de la aplicación.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ejecutando el job de completado de eventos (RN-06).");
        }
    }

    private static async Task<bool> WaitForNextTickAsync(PeriodicTimer timer, CancellationToken cancellationToken)
    {
        try
        {
            return await timer.WaitForNextTickAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
