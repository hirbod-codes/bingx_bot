using bot.src.Data.Models;

namespace bot.src.Brokers;

public interface IBroker
{
    public Task<decimal> GetLastPrice();
    public Task<Candle?> GetCandle(int index);
    public Task<Candle> GetCurrentCandle();
    public Task SetCurrentCandle(Candle candle);
    public Task CandleClosed();
    public Task CloseAllPositions();
    public Task<IEnumerable<Position>> GetOpenPositions();
    public Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice, decimal tpPrice);
    public Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice);
    public Task<IEnumerable<Position>> GetClosedPositions(DateTime start, DateTime? end = null);
}
