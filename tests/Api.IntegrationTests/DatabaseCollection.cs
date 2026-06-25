namespace Ceiba.LiveEvent.Reservations.Api.IntegrationTests;

/// <summary>
/// Comparte una única instancia de <see cref="CustomWebApplicationFactory"/> (y por
/// tanto un único contenedor PostgreSQL) entre todas las pruebas de la colección.
/// </summary>
[CollectionDefinition(nameof(DatabaseCollection))]
public sealed class DatabaseCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
