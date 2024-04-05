using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using bot.src.Brokers.InMemory.Exceptions;
using bot.src.Data.Models;

namespace bot.src.Data.InMemory;

public class PositionRepository : IPositionRepository
{
    private int _lastId = 0;
    private Collection<Position> _positions = new();
    private readonly Collection<Position?> _openPositions = new();
    private bool _anyOpenedPosition = false;
    private bool _anyPendingPosition = false;
    private bool _anyCancelledPosition = false;
    private bool _anyClosedPosition = false;
    private readonly Collection<Position?> _cancelledPositions = new();
    private readonly Collection<Position?> _closedPositions = new();
    private readonly Collection<Position?> _pendingPositions = new();

    private Task<Position> CreatePosition(Position position, string status)
    {
        if (position.CreatedAt == null)
            throw new PositionCreationException();

        position.Id = _lastId.ToString();
        position.PositionStatus = status;
        _positions.Add(position);

        _openPositions.Add(null);
        _cancelledPositions.Add(null);
        _closedPositions.Add(null);
        _pendingPositions.Add(null);

        _lastId++;
        return Task.FromResult(position);
    }

    public async Task<Position> CreateOpenPosition(Position position)
    {
        Position createdPosition = await CreatePosition(position, PositionStatus.OPENED);

        _openPositions[int.Parse(position.Id)] = position;
        _anyOpenedPosition = true;

        return createdPosition;
    }

    public async Task<Position> CreatePendingPosition(Position position)
    {
        Position createdPosition = await CreatePosition(position, PositionStatus.PENDING);

        _pendingPositions[int.Parse(position.Id)] = position;
        _anyPendingPosition = true;

        return createdPosition;
    }

    public Task<bool> AnyOpenedPosition() => Task.FromResult(_anyOpenedPosition);

    public Task<bool> AnyPendingPosition() => Task.FromResult(_anyPendingPosition);

    public Task<bool> AnyCancelledPosition() => Task.FromResult(_anyCancelledPosition);

    public Task<bool> AnyClosedPosition() => Task.FromResult(_anyClosedPosition);

    public Task<Position?> GetCancelledPosition(string id) => Task.FromResult(_cancelledPositions[int.Parse(id)])!;

    public Task<IEnumerable<Position?>> GetCancelledPositions(DateTime start, DateTime? end = null) => Task.FromResult(_cancelledPositions.Where(o =>
        o == null
        ||
        (o.OpenedAt >= start
        && (end == null || o.ClosedAt <= end))
    ));

    public Task<IEnumerable<Position?>> GetCancelledPositions() => Task.FromResult<IEnumerable<Position?>>(_cancelledPositions);

    public Task<Position?> GetClosedPosition(string id) => Task.FromResult(_closedPositions[int.Parse(id)])!;

    public Task<IEnumerable<Position?>> GetClosedPositions(DateTime start, DateTime? end = null) => Task.FromResult(_closedPositions.Where(o =>
        o == null
        ||
        (o.OpenedAt >= start
        && (end == null || o.ClosedAt <= end))
    ));

    public Task<IEnumerable<Position?>> GetClosedPositions() => Task.FromResult<IEnumerable<Position?>>(_closedPositions);

    public Task<Position?> GetOpenedPosition(string id) => Task.FromResult(_openPositions[int.Parse(id)])!;

    public Task<IEnumerable<Position?>> GetOpenedPositions(DateTime start, DateTime? end = null) => Task.FromResult(_openPositions.Where(o =>
        o == null
        ||
        (o.OpenedAt >= start
        && (end == null || o.ClosedAt <= end))
    ));

    public Task<IEnumerable<Position?>> GetOpenedPositions() => Task.FromResult<IEnumerable<Position?>>(_openPositions);

    public Task<Position?> GetPendingPosition(string id) => Task.FromResult(_pendingPositions[int.Parse(id)])!;

    public Task<IEnumerable<Position?>> GetPendingPositions(DateTime start, DateTime? end = null) => Task.FromResult(_pendingPositions.Where(o =>
        o == null
        ||
        (o.OpenedAt >= start
        && (end == null || o.ClosedAt <= end))
    ));

    public Task<IEnumerable<Position?>> GetPendingPositions() => Task.FromResult<IEnumerable<Position?>>(_pendingPositions);

    public Task<Position?> GetPosition(string id) => Task.FromResult(_positions[int.Parse(id)])!;

    public Task<IEnumerable<Position>> GetPositions() => Task.FromResult<IEnumerable<Position>>(_positions);

    public Task<IEnumerable<Position>> GetPositions(DateTime start, DateTime? end = null) => Task.FromResult(_positions.Where(o =>
        o.OpenedAt >= start
        && (end == null || o.ClosedAt <= end)
    ));

    public async Task OpenPosition(string id, DateTime openedAt)
    {
        Position position = await GetPosition(id) ?? throw new PositionNotFoundException();

        if (position.PositionStatus != PositionStatus.PENDING)
            throw new PositionStatusException();

        position.OpenedAt = openedAt;
        position.PositionStatus = PositionStatus.OPENED;

        _openPositions[int.Parse(position.Id)] = position;
        _anyOpenedPosition = true;

        _pendingPositions[int.Parse(position.Id)] = null;

        if (!_pendingPositions.Where(o => o != null).Any())
            _anyPendingPosition = false;
    }

    public async Task ClosePosition(string id, decimal closePrice, DateTime closedAt, decimal brokerCommission, bool unknownState)
    {
        Position position = await GetPosition(id) ?? throw new PositionNotFoundException();

        if (position.PositionStatus != PositionStatus.OPENED)
            throw new ClosingAClosedPosition();

        position.PositionStatus = PositionStatus.CLOSED;

        position.UnknownCloseState = unknownState;

        position.ClosedPrice = closePrice;
        position.ClosedAt = closedAt;

        decimal profit = ((decimal)position.ClosedPrice - position.OpenedPrice) * position.Margin * position.Leverage / position.OpenedPrice;
        if (position.PositionDirection == PositionDirection.SHORT)
            profit *= -1;
        position.Profit = profit;
        decimal commission = brokerCommission * position.Margin * position.Leverage;
        position.Commission = commission;
        position.ProfitWithCommission = profit - commission;

        _closedPositions[int.Parse(position.Id)] = position;
        _anyClosedPosition = true;

        _openPositions[int.Parse(position.Id)] = null;

        if (!_openPositions.Where(o => o != null).Any())
            _anyOpenedPosition = false;
    }

    public async Task CancelPosition(string id, DateTime cancelledAt)
    {
        Position position = await GetPosition(id) ?? throw new PositionNotFoundException();

        if (position.PositionStatus != PositionStatus.PENDING)
            throw new CancellingAPositionException();

        position.CancelledAt = cancelledAt;
        position.PositionStatus = PositionStatus.CANCELLED;

        _cancelledPositions[int.Parse(position.Id)] = position;
        _anyCancelledPosition = true;

        _pendingPositions[int.Parse(position.Id)] = null;

        if (!_pendingPositions.Where(o => o != null).Any())
            _anyPendingPosition = false;
    }
}

[Serializable]
internal class PositionCreationException : Exception
{
    public PositionCreationException()
    {
    }

    public PositionCreationException(string? message) : base(message)
    {
    }

    public PositionCreationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected PositionCreationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}