using bot.src.Data.Models;

namespace bot.src.Data.None;

public class PositionRepository : IPositionRepository
{
    public Task<bool> AnyCancelledPosition() => throw new NotImplementedException();

    public Task<bool> AnyClosedPosition() => throw new NotImplementedException();

    public Task<bool> AnyOpenedPosition() => throw new NotImplementedException();

    public Task<bool> AnyPendingPosition() => throw new NotImplementedException();

    public Task CancelPosition(string id, DateTime cancelledAt) => throw new NotImplementedException();

    public Task ClosePosition(string id, decimal closePrice, DateTime closedAt, decimal brokerCommission, bool unknownState) => throw new NotImplementedException();

    public Task<Position> CreateOpenPosition(Position position) => throw new NotImplementedException();

    public Task<Position> CreatePendingPosition(Position position) => throw new NotImplementedException();

    public Task<Position?> GetCancelledPosition(string id) => throw new NotImplementedException();

    public Task<IEnumerable<Position?>> GetCancelledPositions(DateTime startTime, DateTime? endTime = null) => throw new NotImplementedException();

    public Task<IEnumerable<Position?>> GetCancelledPositions() => throw new NotImplementedException();

    public Task<Position?> GetClosedPosition(string id) => throw new NotImplementedException();

    public Task<IEnumerable<Position?>> GetClosedPositions(DateTime startTime, DateTime? endTime = null) => throw new NotImplementedException();

    public Task<IEnumerable<Position?>> GetClosedPositions() => throw new NotImplementedException();

    public Task<Position?> GetOpenedPosition(string id) => throw new NotImplementedException();

    public Task<IEnumerable<Position?>> GetOpenedPositions(DateTime startTime, DateTime? endTime = null) => throw new NotImplementedException();

    public Task<IEnumerable<Position?>> GetOpenedPositions() => throw new NotImplementedException();

    public Task<Position?> GetPendingPosition(string id) => throw new NotImplementedException();

    public Task<IEnumerable<Position?>> GetPendingPositions(DateTime startTime, DateTime? endTime = null) => throw new NotImplementedException();

    public Task<IEnumerable<Position?>> GetPendingPositions() => throw new NotImplementedException();

    public Task<Position?> GetPosition(string id) => throw new NotImplementedException();

    public Task<IEnumerable<Position>> GetPositions() => throw new NotImplementedException();

    public Task<IEnumerable<Position>> GetPositions(DateTime start, DateTime? end = null) => throw new NotImplementedException();

    public Task OpenPosition(string id) => throw new NotImplementedException();

    public Task ReplacePosition(Position position) => throw new NotImplementedException();
}
