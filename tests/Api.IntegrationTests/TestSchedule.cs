namespace Ceiba.LiveEvent.Reservations.Api.IntegrationTests;

/// <summary>
/// Genera franjas horarias deterministas y únicas para los eventos de prueba: lunes a las
/// 10:00, una semana de separación. Así se evita disparar RN-02 (superposición en el venue),
/// RN-03 (fin de semana nocturno) y RN-04 (inicio inminente) en escenarios de éxito.
/// </summary>
internal static class TestSchedule
{
    private static int _counter;

    public static (DateTimeOffset Start, DateTimeOffset End) NextSlot()
    {
        var n = Interlocked.Increment(ref _counter);

        var monday = DateTime.UtcNow.Date.AddDays(14);
        while (monday.DayOfWeek != DayOfWeek.Monday)
        {
            monday = monday.AddDays(1);
        }

        var day = monday.AddDays(7 * n);
        var start = new DateTimeOffset(day.Year, day.Month, day.Day, 10, 0, 0, TimeSpan.Zero);
        return (start, start.AddHours(3));
    }
}
