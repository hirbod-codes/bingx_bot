using bot.src.Brokers.InMemory.Exceptions;
using bot.src.Data.Models;

namespace bot.src.Data.InMemory;

public class PositionRepository : IPositionRepository
{
    private int _lastId = 0;
    private List<Position> _positions = new();
    private readonly List<Position?> _openPositions = new();
    private bool _anyOpenedPosition = false;
    private readonly List<Position?> _cancelledPositions = new();
    private readonly List<Position?> _closedPositions = new();
    private readonly List<Position?> _pendingPositions = new();

    public Task<Position> CreatePosition(Position position)
    {
        position.Id = _lastId.ToString();
        _positions.Add(position);

        _openPositions.Add(position);
        _anyOpenedPosition = true;

        _cancelledPositions.Add(null);
        _closedPositions.Add(null);
        _pendingPositions.Add(null);

        _lastId++;

        return Task.FromResult(position);
    }

    public Task<bool> AnyOpenedPosition() => Task.FromResult(_anyOpenedPosition);

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

    public async Task ClosePosition(string id, decimal closePrice, DateTime closedAt, decimal brokerCommission, bool unknownState)
    {
        Position position = await GetPosition(id) ?? throw new PositionNotFoundException();

        if (position.PositionStatus == PositionStatus.CLOSED)
            throw new ClosingAClosedPosition();

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

        switch (position.PositionStatus)
        {
            case PositionStatus.OPENED:
                _openPositions[int.Parse(position.Id)] = null;
                break;
            case PositionStatus.PENDING:
                _pendingPositions[int.Parse(position.Id)] = null;
                break;
            case PositionStatus.CANCELLED:
                _cancelledPositions[int.Parse(position.Id)] = null;
                break;
            default:
                throw new InvalidPositionStatusException();
        }

        position.PositionStatus = PositionStatus.CLOSED;

        _closedPositions[int.Parse(position.Id)] = position;

        if (!_openPositions.Where(o => o != null).Any())
            _anyOpenedPosition = false;
    }
}
