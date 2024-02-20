using bot.src.Data.Models;

namespace bot.src.Data;

public interface IPositionRepository
{
    public Task<Position> CreatePosition(Position position);
    public Task<Position?> GetPosition(string id);
    public Task<Position?> GetOpenedPosition(string id);
    public Task<Position?> GetClosedPosition(string id);
    public Task<Position?> GetPendingPosition(string id);
    public Task<Position?> GetCancelledPosition(string id);
    public Task<IEnumerable<Position?>> GetOpenedPositions(DateTime startTime, DateTime? endTime = null);
    public Task<IEnumerable<Position?>> GetClosedPositions(DateTime startTime, DateTime? endTime = null);
    public Task<IEnumerable<Position?>> GetPendingPositions(DateTime startTime, DateTime? endTime = null);
    public Task<IEnumerable<Position?>> GetCancelledPositions(DateTime startTime, DateTime? endTime = null);
    public Task<IEnumerable<Position?>> GetOpenedPositions();
    public Task<IEnumerable<Position?>> GetClosedPositions();
    public Task<IEnumerable<Position?>> GetPendingPositions();
    public Task<IEnumerable<Position?>> GetCancelledPositions();
    public Task<IEnumerable<Position>> GetPositions();
    public Task<IEnumerable<Position>> GetPositions(DateTime start, DateTime? end = null);
    public Task ClosePosition(string id, decimal closePrice, DateTime closedAt, decimal brokerCommission);
    public Task<bool> AnyOpenedPosition();
}
