namespace strategies.src.EMA;

public class EMAStrategy : IStrategy
{
    public EMASignals Signals { get; private set; }

    public EMAStrategy(EMASignals signals) => Signals = signals;

    public async Task Initiate() => await Signals.Initiate();

    public bool CheckClosePositionSignal(bool? isLastOpenPositionLong) => isLastOpenPositionLong switch
    {
        null => false,
        true => Signals.CheckShortSignal(),
        false => Signals.CheckLongSignal()
    };

    public bool? CheckOpenPositionSignal(bool? isLastOpenPositionLong) => isLastOpenPositionLong switch
    {
        null => Signals.CheckLongSignal() ? true : (Signals.CheckShortSignal() ? false : null),
        true => Signals.CheckShortSignal(),
        false => Signals.CheckLongSignal()
    };
}
