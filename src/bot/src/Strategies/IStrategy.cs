using bot.src.Data.Models;

namespace bot.src.Strategies;

public interface IStrategy
{
    public Task HandleCandle(Candle candle, int timeFrame);
}
