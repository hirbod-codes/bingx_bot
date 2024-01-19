using bot.src.Data.Models;

namespace bot.src.Brokers;

public interface IBroker
{
    public Task CandleClosed();
    public Task CloseAllPositions();
    public Task<IEnumerable<Position>> GetOpenPositions();
    public Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice, decimal tpPrice);
    public Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice);
}
