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

    public async Task<bool> CheckClosePositionSignal(bool? isLastOpenPositionLong, int timeFrame)
    {
        Logger.Information("Checking for close position...");
        bool r = false;

        if (isLastOpenPositionLong is null)
        {
            Logger.Information("Result is {result}.(isLastOpenPositionLong is null)", r);
            Logger.Information("Finished checking for close position...");
            return r;
        }

        SignalProvider.ResetSignals();

        bool signal = await SignalProvider.CheckSignals() && SignalProvider.GetSignalTime() >= DateTime.UtcNow.AddMinutes(-1);

        if (signal && (bool)isLastOpenPositionLong == !SignalProvider.IsSignalLong())
            r = true;

        Logger.Information("Result is {result}", r);
        Logger.Information("Finished checking for close position...");
        return r;
    }

    public async Task<bool> CheckOpenPositionSignal(bool? isLastOpenPositionLong, int timeFrame)
    {
        Logger.Information("Checking for open position...");
        SignalProvider.ResetSignals();

        bool r = await SignalProvider.CheckSignals() &&
            SignalProvider.GetSignalTime() >= DateTime.UtcNow.AddMinutes(-1) &&
            (isLastOpenPositionLong is null || isLastOpenPositionLong is not null && SignalProvider.IsSignalLong() == !isLastOpenPositionLong);

        Logger.Information("Result is {result}", r);
        Logger.Information("Finished checking for open position...");
        return r;
    }

    public ISignalProvider GetLastSignal() => SignalProvider;

    public async Task Initiate() => await SignalProvider.Initiate();
}
