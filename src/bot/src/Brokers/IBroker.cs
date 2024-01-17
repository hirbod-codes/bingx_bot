using bot.src.Data.Models;

namespace bot.src.Brokers;

public interface IBroker
{
    public Task CandleClosed(Candle candle);
    public Task CandleClosed(int index);
    public Task CloseAllPositions();
    public Task<IEnumerable<Position>> GetOpenPositions();
    public Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice, decimal tpPrice);
    public Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice);
}
