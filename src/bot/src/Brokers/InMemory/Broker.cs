using bot.src.Broker.InMemory;
using bot.src.Brokers.InMemory.Exceptions;
using bot.src.Data;
using bot.src.Data.Models;
using Serilog;

namespace bot.src.Brokers.InMemory;

public class Broker : IBroker
{
    private readonly IPositionRepository _positionRepository;
    private readonly ICandleRepository _candleRepository;
    private readonly ILogger _logger;
    private readonly BrokerOptions _brokerOptions;

    public Broker(IBrokerOptions brokerOptions, IPositionRepository positionRepository, ICandleRepository candleRepository, ILogger logger)
    {
        _candleRepository = candleRepository;
        _logger = logger.ForContext<Broker>();
        _brokerOptions = (brokerOptions as BrokerOptions)!;
        _positionRepository = positionRepository;
    }

    public Task InitiateCandleStore(int candlesCount = 10000)
    {
        throw new NotImplementedException();
    }

    public Task<Candles> GetCandles()
    {
        throw new NotImplementedException();
    }

    public async Task<Candle> GetCandle(int indexFromEnd = 0) => await _candleRepository.GetCandle(indexFromEnd);

    public async Task<decimal> GetLastPrice() => (await GetCandle()).Close;

    public async Task CandleClosed()
    {
        _logger.Information("Getting open positions...");

        IEnumerable<Position> openPositions = await _positionRepository.GetOpenedPositions();

        if (!openPositions.Any())
        {
            _logger.Information("There is no open position.");
            return;
        }

        _logger.Information("Closing open positions that are suppose to be closed.");

        int closedPositionsCount = 0;
        foreach (Position position in openPositions)
            if (await ShouldClosePosition(position))
            {
                closedPositionsCount++;
                await ClosePosition(position);
            }
        _logger.Information("{closedPositionsCount} positions closed.", closedPositionsCount);
    }

    private async Task<bool> ShouldClosePosition(Position position) =>
        (
            position.PositionDirection == PositionDirection.LONG &&
            ((await GetCandle()).Low <= position.SLPrice || (position.TPPrice != null && (await GetCandle()).High >= position.TPPrice))
        ) ||
        (
            position.PositionDirection == PositionDirection.SHORT &&
            ((await GetCandle()).High >= position.SLPrice || (position.TPPrice != null && (await GetCandle()).Low <= position.TPPrice))
        );

    public async Task ClosePosition(Position position)
    {
        decimal? closedPrice = null!;
        if (position.PositionDirection == PositionDirection.LONG)
        {
            if ((await GetCandle()).Low <= position.SLPrice)
                closedPrice = position.SLPrice;
            else if (position.TPPrice != null && (await GetCandle()).High >= position.TPPrice)
                closedPrice = (decimal)position.TPPrice;
        }
        else if (position.PositionDirection == PositionDirection.SHORT)
        {
            if ((await GetCandle()).High >= position.SLPrice)
                closedPrice = position.SLPrice;
            else if (position.TPPrice != null && (await GetCandle()).Low <= position.TPPrice)
                closedPrice = (decimal)position.TPPrice;
        }
        else
            throw new ClosePositionException();

        await ClosePosition(position.Id, (decimal)closedPrice, (await GetCandle()).Date.AddSeconds(await _candleRepository.GetTimeFrame()));
    }

    public async Task CloseAllPositions()
    {
        Candle candle = await GetCandle();
        await CloseAllPositions(candle.Close, candle.Date.AddSeconds(await _candleRepository.GetTimeFrame()));
    }

    public async Task CloseAllPositions(decimal closedPrice, DateTime closedAt)
    {
        IEnumerable<Position> openPositions = await GetOpenPositions();

        foreach (Position position in openPositions)
            await ClosePosition(position.Id, closedPrice, closedAt);
    }

    public async Task ClosePosition(string id, decimal closedPrice, DateTime closedAt)
    {
        Position position = await _positionRepository.GetPosition(id) ?? throw new PositionNotFoundException();

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

    public async Task<IEnumerable<Position>> GetOpenPositions() => await _positionRepository.GetOpenedPositions();

    public Task<IEnumerable<Position>> GetClosedPositions(DateTime start, DateTime? end = null) => _positionRepository.GetClosedPositions(start, end);

    public async Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice, decimal tpPrice) => await _positionRepository.CreatePosition(new Position()
    {
        Leverage = leverage,
        Margin = margin,
        OpenedAt = (await GetCandle()).Date.AddSeconds(await _candleRepository.GetTimeFrame()),
        OpenedPrice = (await GetCandle()).Close,
        SLPrice = slPrice,
        TPPrice = tpPrice,
        CommissionRatio = _brokerOptions.BrokerCommission,
        Symbol = _brokerOptions.Symbol,
        PositionDirection = direction,
    });

    public async Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice) => await _positionRepository.CreatePosition(new Position()
    {
        Leverage = leverage,
        Margin = margin,
        OpenedAt = (await GetCandle()).Date.AddSeconds(await _candleRepository.GetTimeFrame()),
        OpenedPrice = (await GetCandle()).Close,
        SLPrice = slPrice,
        CommissionRatio = _brokerOptions.BrokerCommission,
        Symbol = _brokerOptions.Symbol,
        PositionDirection = direction,
    });
}
