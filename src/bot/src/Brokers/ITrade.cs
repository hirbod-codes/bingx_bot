using bot.src.Data.Models;

namespace bot.src.Brokers;

public interface ITrade
{
    public Task OpenMarketPosition(Position position);
    public Task CloseAllPositions(Candle candle);
    public Task ClosePosition(string id, decimal closedPrice, DateTime closedAt);
    public Task CloseOpenPositions(Candle currentCandle);
    public Task<IEnumerable<Position>> GetOpenPositions();
    public Task<IEnumerable<Position>> GetOpenPositions(DateTime start, DateTime? end = null);
    public Task<IEnumerable<Position>> GetClosedPositions();
    public Task<IEnumerable<Position>> GetClosedPositions(DateTime start, DateTime? end = null);
    public Task<IEnumerable<Position>> GetAllPositions();
    public Task<IEnumerable<Position>> GetAllPositions(DateTime start, DateTime? end = null);
}
