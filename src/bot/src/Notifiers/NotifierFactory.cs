using ILogger = Serilog.ILogger;
using NtfyNotifier = bot.src.Notifiers.NTFY.Notifier;
using InMemoryNotifier = bot.src.Notifiers.InMemory.Notifier;
using bot.src.Data;

namespace bot.src.Notifiers;

public static class NotifierFactory
{
    public static INotifier CreateNotifier(string notifierName, IMessageRepository messageRepository, ILogger logger) => notifierName switch
    {
        NotifierNames.NTFY => new NtfyNotifier(logger),
        NotifierNames.IN_MEMORY => new InMemoryNotifier(messageRepository, logger),
        _ => throw new Exception()
    };

    public static Type? GetInstanceType(string? name) => name switch
    {
        null => null,
        NotifierNames.NTFY => typeof(NtfyNotifier),
        NotifierNames.IN_MEMORY => typeof(InMemoryNotifier),
        _ => throw new Exception()
    };
}
