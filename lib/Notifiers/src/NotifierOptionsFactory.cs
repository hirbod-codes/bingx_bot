using Abstractions.src.Notifiers;

namespace Notifiers.src;

public static class NotifierOptionsFactory
{
    public static INotifierOptions CreateNotifierOptions(string optionsName) => optionsName switch
    {
        NotifierNames.IN_MEMORY => new InMemory.NotifierOptions(),
        NotifierNames.NTFY => new NTFY.NotifierOptions(),
        _ => throw new Exception("Invalid Notifier option name provided.")
    };

    public static Type? GetInstanceType(string? name) => name switch
    {
        null => null,
        NotifierNames.IN_MEMORY => typeof(InMemory.NotifierOptions),
        NotifierNames.NTFY => typeof(NTFY.NotifierOptions),
        _ => throw new Exception()
    };
}
