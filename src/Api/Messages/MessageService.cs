namespace Ceiba.LiveEvent.Reservations.Api.Messages;

/// <summary>
/// Implementación en memoria de <see cref="IMessageService"/>. El catálogo se carga una
/// sola vez al arrancar (ver <see cref="MessageCatalogExtensions"/>).
/// </summary>
public sealed class MessageService : IMessageService
{
    private readonly IReadOnlyDictionary<string, string> _messages;

    public MessageService(IReadOnlyDictionary<string, string> messages) => _messages = messages;

    public string Get(string key)
        => _messages.TryGetValue(key, out var message) ? message : $"[{key}]";

    public string Get(string key, params object?[] args)
    {
        var template = Get(key);
        return args.Length > 0 ? string.Format(template, args) : template;
    }
}
