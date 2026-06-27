using Ceiba.LiveEvent.Reservations.Domain.Common;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ceiba.LiveEvent.Reservations.Infrastructure.Persistence.Converters;

/// <summary>Convierte el value object <see cref="Email"/> a/desde su representación en texto.</summary>
public sealed class EmailConverter : ValueConverter<Email, string>
{
    public EmailConverter() : base(e => e.Value, v => Email.FromTrusted(v))
    {
    }
}
