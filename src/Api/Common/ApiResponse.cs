namespace Ceiba.LiveEvent.Reservations.Api.Common;

/// <summary>
/// Envoltura estándar para las respuestas de los endpoints de comando: un mensaje de
/// proceso (tomado del catálogo) y, opcionalmente, los datos resultantes.
/// </summary>
/// <typeparam name="T">Tipo de la carga útil (por ejemplo, el id generado).</typeparam>
/// <remarks>
/// El parámetro <c>Data</c> no lleva valor por defecto a propósito: con un genérico sin
/// restricción, <c>= default</c> rompe la generación del esquema OpenAPI al cerrar sobre un
/// tipo de valor (p. ej. <see cref="System.Guid"/>). Use <see cref="Of"/> para omitir los datos.
/// </remarks>
public sealed record ApiResponse<T>(string Message, T? Data)
{
    public static ApiResponse<T> Of(string message, T? data = default) => new(message, data);
}
