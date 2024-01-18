using bot.src.Brokers;
using bot.src.Brokers.InMemory;
using bot.src.Brokers.InMemory.Exceptions;
using bot.src.Data;
using bot.src.Data.Models;
using Serilog;

namespace bot.src.Broker.InMemory;

public class Trade : ITrade
{
    private readonly BrokerOptions _brokerOptions;
    private readonly ICandleRepository _candleRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly ILogger _logger;

    public Trade(BrokerOptions brokerOptions, ICandleRepository candleRepository, IPositionRepository positionRepository, ILogger logger)
    {
        _brokerOptions = brokerOptions;
        _candleRepository = candleRepository;
        _positionRepository = positionRepository;
        _logger = logger.ForContext<Trade>();
    }

    public async Task OpenMarketPosition(Position position) => await _positionRepository.CreatePosition(position);

    public async Task CloseAllPositions(Candle candle) => await CloseAllPositions(candle.Close, candle.Date.AddSeconds(await _candleRepository.GetTimeFrame()));

    public async Task CloseAllPositions(decimal closedPrice, DateTime closedAt)
    {
        IEnumerable<Position> openPositions = await GetOpenPositions();

        foreach (Position position in openPositions)
            await ClosePosition(position.Id, closedPrice, closedAt);
    }

    public async Task ClosePosition(string id, decimal closedPrice, DateTime closedAt)
    {
        Position position = await GetPosition(id) ?? throw new PositionNotFoundException();

        if (position.PositionStatus == PositionStatus.CLOSED)
            throw new ClosingAClosedPosition();

        position.ClosedPrice = closedPrice;
        position.ClosedAt = closedAt;

        decimal? profit = (position.ClosedPrice - position.OpenedPrice) * position.Margin * position.Leverage / position.OpenedPrice;
        if (position.PositionDirection == PositionDirection.SHORT)
            profit *= -1;
        position.Profit = profit;
        decimal commission = _brokerOptions.BrokerCommission * position.Margin * position.Leverage;
        position.Commission = commission;
        position.ProfitWithCommission = profit - commission;

        position.PositionStatus = PositionStatus.CLOSED;

        await _positionRepository.ReplacePosition(position);
    }

    public async Task<IEnumerable<Position>> GetPositions() => await _positionRepository.GetPositions();

    public async Task<IEnumerable<Position>> GetPositions(DateTime start, DateTime? end = null) => await _positionRepository.GetPositions(start, end);

    public async Task<IEnumerable<Position>> GetClosedPositions() => await _positionRepository.GetClosedPositions();

    public async Task<IEnumerable<Position>> GetClosedPositions(DateTime start, DateTime? end = null) => await _positionRepository.GetClosedPositions(start, end);

    public async Task<IEnumerable<Position>> GetOpenPositions() => await _positionRepository.GetOpenedPositions();

    public async Task<IEnumerable<Position>> GetOpenPositions(DateTime start, DateTime? end = null) => await _positionRepository.GetOpenedPositions(start, end);

    public async Task<Position?> GetPosition(string id) => await _positionRepository.GetPosition(id);

}
