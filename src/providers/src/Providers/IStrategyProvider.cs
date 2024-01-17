using bot.src.Data.Models;

namespace providers.src.Providers;

public interface IStrategyProvider
{
    public Task Reset();
    public Task<Candle> GetClosedCandle();
    public Task<bool> TryMoveToNextCandle();
}
