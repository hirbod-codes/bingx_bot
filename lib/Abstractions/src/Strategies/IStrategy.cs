using Abstractions.src.Models;

namespace Abstractions.src.Strategies;

public interface IStrategy
{
    public Task PrepareIndicators();
    public Dictionary<string, object> GetIndicators();
    public Task HandleCandle(Candle candle, int timeFrame);
}
