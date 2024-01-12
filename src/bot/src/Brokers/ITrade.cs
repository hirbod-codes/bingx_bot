
using bot.src.Strategies.Models;

namespace bot.src.Brokers;

public interface ITrade
{
    public Task<string> GetSymbol();
    public Task SetSymbol(string symbol);
    public Task<float> GetPrice();
    public Task<float> GetLeverage();
    public Task SetLeverage(float leverage);
    public Task OpenMarketOrder(float quantity, bool direction, float slPrice, float tpPrice);
    public Task OpenMarketOrder(float quantity, bool direction, float slPrice);
    public Task CloseAllPositions();
    public Task ClosePosition(string id);
    public Task<Position> GetPosition(string id);
    public Task<IEnumerable<Position>> GetOpenPositions();
    public Task<IEnumerable<Position>> GetOpenPositions(DateTime start, DateTime? end = null);
    public Task<IEnumerable<Position>> GetClosedPositions();
    public Task<IEnumerable<Position>> GetClosedPositions(DateTime start, DateTime? end = null);
    public Task<IEnumerable<Position>> GetAllPositions();
    public Task<IEnumerable<Position>> GetAllPositions(DateTime start, DateTime? end = null);
}
