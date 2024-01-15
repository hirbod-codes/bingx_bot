
using bot.src.Data.Models;

namespace bot.src.Data.InMemory;

public class PositionRepository : IPositionRepository
{
    private IEnumerable<Position> _positions = Array.Empty<Position>();

    public Task CreatePosition(decimal openedPrice, decimal margin, decimal leverage, decimal slPrice, decimal tpPrice, DateTime openedAt)
    {
        _positions = _positions.Append(new()
        {
            Id = _positions.Any() ? int.Parse(_positions.Last().Id + 1).ToString() : "0",
            OpenedPrice = openedPrice,
            Margin = margin,
            Leverage = leverage,
            SLPrice = slPrice,
            TPPrice = tpPrice,
            OpenedAt = openedAt
        });
        return Task.CompletedTask;
    }

    public Task CreatePosition(decimal openedPrice, decimal margin, decimal leverage, decimal slPrice, DateTime openedAt)
    {
        _positions = _positions.Append(new()
        {
            Id = _positions.Any() ? int.Parse(_positions.Last().Id + 1).ToString() : "0",
            OpenedPrice = openedPrice,
            Margin = margin,
            Leverage = leverage,
            SLPrice = slPrice,
            OpenedAt = openedAt
        });
        return Task.CompletedTask;
    }

    public Task<Position?> GetCancelledPosition(string id) => Task.FromResult(_positions.FirstOrDefault(o => o.Id == id && o.PositionStatus == PositionStatus.CANCELLED));

    public Task<IEnumerable<Position>> GetCancelledPositions(DateTime start, DateTime? end = null) => Task.FromResult(_positions.Where(o =>
        o.PositionStatus == PositionStatus.CANCELLED
        && o.OpenedAt >= start
        && (end == null || o.ClosedAt <= end)
    ));

    public Task<IEnumerable<Position>> GetCancelledPositions() => Task.FromResult(_positions.Where(o => o.PositionStatus == PositionStatus.CANCELLED));

    public Task<Position?> GetClosedPosition(string id) => Task.FromResult(_positions.FirstOrDefault(o => o.Id == id && o.PositionStatus == PositionStatus.CLOSED));

    public Task<IEnumerable<Position>> GetClosedPositions(DateTime start, DateTime? end = null) => Task.FromResult(_positions.Where(o =>
        o.PositionStatus == PositionStatus.CLOSED
        && o.OpenedAt >= start
        && (end == null || o.ClosedAt <= end)
    ));

    public Task<IEnumerable<Position>> GetClosedPositions() => Task.FromResult(_positions.Where(o => o.PositionStatus == PositionStatus.CLOSED));

    public Task<Position?> GetOpenedPosition(string id) => Task.FromResult(_positions.FirstOrDefault(o => o.Id == id && o.PositionStatus == PositionStatus.OPENED));

    public Task<IEnumerable<Position>> GetOpenedPositions(DateTime start, DateTime? end = null) => Task.FromResult(_positions.Where(o =>
        o.PositionStatus == PositionStatus.OPENED
        && o.OpenedAt >= start
        && (end == null || o.ClosedAt <= end)
    ));

    public Task<IEnumerable<Position>> GetOpenedPositions() => Task.FromResult(_positions.Where(o => o.PositionStatus == PositionStatus.OPENED));

    public Task<Position?> GetPendingPosition(string id) => Task.FromResult(_positions.FirstOrDefault(o => o.Id == id && o.PositionStatus == PositionStatus.PENDING));

    public Task<IEnumerable<Position>> GetPendingPositions(DateTime start, DateTime? end = null) => Task.FromResult(_positions.Where(o =>
        o.PositionStatus == PositionStatus.PENDING
        && o.OpenedAt >= start
        && (end == null || o.ClosedAt <= end)
    ));

    public Task<IEnumerable<Position>> GetPendingPositions() => Task.FromResult(_positions.Where(o => o.PositionStatus == PositionStatus.PENDING));

    public Task<Position?> GetPosition(string id) => Task.FromResult(_positions.FirstOrDefault(o => o.Id == id));

    public Task<IEnumerable<Position>> GetPositions() => Task.FromResult(_positions);

    public Task<IEnumerable<Position>> GetPositions(DateTime start, DateTime? end = null) => Task.FromResult(_positions.Where(o =>
        o.OpenedAt >= start
        && (end == null || o.ClosedAt <= end)
    ));

    public Task ReplacePosition(Position position)
    {
        _positions = _positions.Where(o => o.Id != position.Id);
        _positions = _positions.Append(position);
        return Task.CompletedTask;
    }
}
