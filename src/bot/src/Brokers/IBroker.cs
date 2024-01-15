using bot.src.Data.Models;

namespace bot.src.Broker;

public interface IBroker
{
    public event EventHandler? CandleProcessed;
    public Task CandleClosed(Candle candle);
    public Task CandleClosed(int index);
}
