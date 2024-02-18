using bot.src.Data.Models;

namespace bot.src.Brokers;

public interface IBroker
{
    public Task<decimal> GetLastPrice();
    public Task<Candle> GetCandle(int indexFromEnd = 0);
    public Task InitiateCandleStore(int candlesCount = 10000);
    public Task<Candles> GetCandles();
    public Task CandleClosed();
    public Task CloseAllPositions();
    public Task ClosePosition(Position position);
    public Task<IEnumerable<Position>> GetOpenPositions();
    public Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice, decimal tpPrice);
    public Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice);
    public Task<IEnumerable<Position>> GetClosedPositions(DateTime start, DateTime? end = null);
    public Task<int> GetLastCandleIndex();
}
