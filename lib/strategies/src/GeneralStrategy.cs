using Serilog;

namespace email_api.src;

public class GeneralStrategy : IStrategy
{
    public ILogger Logger { get; }
    private ISignalProvider SignalProvider { get; set; }

    public GeneralStrategy(ISignalProvider signalProvider, ILogger logger)
    {
        SignalProvider = signalProvider;
        Logger = logger;
    }

    public async Task<bool> CheckClosePositionSignal(bool? isLastOpenPositionLong)
    {
        if (isLastOpenPositionLong is null)
            return false;

        SignalProvider.ResetSignals();

        bool signal = await SignalProvider.CheckSignals() && SignalProvider.GetSignalTime() >= DateTime.UtcNow.AddMinutes(-1);

        if (signal && ((bool)isLastOpenPositionLong ? !SignalProvider.IsSignalLong() : SignalProvider.IsSignalLong()))
            return true;

        return false;
    }

    public async Task<bool> CheckOpenPositionSignal(bool? isLastOpenPositionLong)
    {
        if (isLastOpenPositionLong is not null)
            return false;

        SignalProvider.ResetSignals();

        return await SignalProvider.CheckSignals() && SignalProvider.GetSignalTime() >= DateTime.UtcNow.AddMinutes(-1);
    }

    public ISignalProvider GetLastSignal() => SignalProvider;

    public async Task Initiate() => await SignalProvider.Initiate();
}
