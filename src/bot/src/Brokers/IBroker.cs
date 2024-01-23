using bot.src.Data.Models;

namespace bot.src.Brokers;

public interface IBroker
{
    public Task<Candle> GetCurrentCandle();
    public Task SetCurrentCandle(Candle candle);
    public Task CandleClosed();
    public Task CloseAllPositions();
    public Task<IEnumerable<Position>> GetOpenPositions();
    public Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice, decimal tpPrice);
    public Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice);
    public Task<IEnumerable<Position>> GetClosedPositions(DateTime start, DateTime? end = null);
    public Task<IEnumerable<Position>> GetOpenedPositions();
}
