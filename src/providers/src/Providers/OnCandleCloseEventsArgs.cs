using bot.src.Data.Models;

namespace providers.src.Providers;

public class OnCandleCloseEventsArgs
{
    public Candle Candle { get; set; } = null!;

    public OnCandleCloseEventsArgs(Candle candle) => Candle = candle;
}
