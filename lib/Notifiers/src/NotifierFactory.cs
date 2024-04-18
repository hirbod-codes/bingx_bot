using ILogger = Serilog.ILogger;
using NtfyNotifier = Notifiers.src.NTFY.Notifier;
using InMemoryNotifier = Notifiers.src.InMemory.Notifier;
using Abstractions.src.Notifiers;
using Abstractions.src.Repository;

namespace Notifiers.src;

public static class NotifierFactory
{
    public static INotifier CreateNotifier(string notifierName, IMessageRepository messageRepository, ILogger logger, INotifierOptions notifierOptions) => notifierName switch
    {
        NotifierNames.NTFY => new NtfyNotifier(notifierOptions, logger),
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
