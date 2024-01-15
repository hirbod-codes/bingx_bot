using bot.src.Data.Models;

namespace bot.src.Brokers;

public interface ITrade
{
    public Task<string> GetSymbol();
    public Task SetSymbol(string symbol);
    public Task<decimal> GetPrice();
    public Task<decimal> GetLeverage();
    public Task SetLeverage(decimal leverage);
    public Task OpenMarketOrder(decimal margin, bool direction, decimal slPrice, decimal tpPrice);
    public Task OpenMarketOrder(decimal margin, bool direction, decimal slPrice);
    public Task CloseAllPositions();
    public Task ClosePosition(string id);
    public Task<Position?> GetPosition(string id);
    public Task<IEnumerable<Position>> GetOpenPositions();
    public Task<IEnumerable<Position>> GetOpenPositions(DateTime start, DateTime? end = null);
    public Task<IEnumerable<Position>> GetClosedPositions();
    public Task<IEnumerable<Position>> GetClosedPositions(DateTime start, DateTime? end = null);
    public Task<IEnumerable<Position>> GetAllPositions();
    public Task<IEnumerable<Position>> GetAllPositions(DateTime start, DateTime? end = null);
}
