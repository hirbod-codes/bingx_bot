using bot.src.Brokers.InMemory.Exceptions;
using bot.src.Data;
using bot.src.Data.Models;
using Serilog;

namespace bot.src.Brokers.InMemory;

public class Broker : IBroker
{
    private readonly IPositionRepository _positionRepository;
    private readonly ICandleRepository _candleRepository;
    private readonly IAccount _account;
    private readonly ITrade _trade;
    private readonly ILogger _logger;
    private readonly BrokerOptions _brokerOptions;

    public Broker(IBrokerOptions brokerOptions, ITrade trade, IAccount account, IPositionRepository positionRepository, ICandleRepository candleRepository, ILogger logger)
    {
        _trade = trade;
        _candleRepository = candleRepository;
        _logger = logger.ForContext<Broker>();
        _brokerOptions = (brokerOptions as BrokerOptions)!;
        _account = account;
        _positionRepository = positionRepository;
    }

    public async Task CandleClosed()
    {
        _logger.Information("Getting open positions...");

        IEnumerable<Position> openPositions = await _trade.GetOpenPositions();

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
            ((await GetCurrentCandle()).Low <= position.SLPrice || (position.TPPrice != null && (await GetCurrentCandle()).High >= position.TPPrice))
        ) ||
        (
            position.PositionDirection == PositionDirection.SHORT &&
            ((await GetCurrentCandle()).High >= position.SLPrice || (position.TPPrice != null && (await GetCurrentCandle()).Low <= position.TPPrice))
        );

    private async Task ClosePosition(Position position)
    {
        decimal? closedPrice = null!;
        if (position.PositionDirection == PositionDirection.LONG)
        {
            if ((await GetCurrentCandle()).Low <= position.SLPrice)
                closedPrice = position.SLPrice;
            else if (position.TPPrice != null && (await GetCurrentCandle()).High >= position.TPPrice)
                closedPrice = (decimal)position.TPPrice;
        }
        else if (position.PositionDirection == PositionDirection.SHORT)
        {
            if ((await GetCurrentCandle()).High >= position.SLPrice)
                closedPrice = position.SLPrice;
            else if (position.TPPrice != null && (await GetCurrentCandle()).Low <= position.TPPrice)
                closedPrice = (decimal)position.TPPrice;
        }
        else
            throw new ClosePositionException();

        await _trade.ClosePosition(position.Id, (decimal)closedPrice, (await GetCurrentCandle()).Date.AddSeconds(await _candleRepository.GetTimeFrame()));
    }

    public Task<Candle> GetCurrentCandle() => Task.FromResult(_candleRepository.GetCurrentCandle());

    public Task SetCurrentCandle(Candle candle)
    {
        _candleRepository.SetCurrentCandle(candle);
        return Task.CompletedTask;
    }

    public async Task CloseAllPositions() => await _trade.CloseAllPositions(await GetCurrentCandle());

    public async Task<IEnumerable<Position>> GetOpenPositions() => await _trade.GetOpenPositions();

    public async Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice, decimal tpPrice) => await _trade.OpenMarketPosition(new Position()
    {
        Leverage = leverage,
        Margin = margin,
        OpenedAt = (await GetCurrentCandle()).Date.AddSeconds(await _candleRepository.GetTimeFrame()),
        OpenedPrice = (await GetCurrentCandle()).Close,
        SLPrice = slPrice,
        TPPrice = tpPrice,
        CommissionRatio = _brokerOptions.BrokerCommission,
        Symbol = _brokerOptions.Symbol,
        PositionDirection = direction,
    });

    public async Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice) => await _trade.OpenMarketPosition(new Position()
    {
        Leverage = leverage,
        Margin = margin,
        OpenedAt = (await GetCurrentCandle()).Date.AddSeconds(await _candleRepository.GetTimeFrame()),
        OpenedPrice = (await GetCurrentCandle()).Close,
        SLPrice = slPrice,
        CommissionRatio = _brokerOptions.BrokerCommission,
        Symbol = _brokerOptions.Symbol,
        PositionDirection = direction,
    });

    public Task<IEnumerable<Position>> GetClosedPositions(DateTime start, DateTime? end = null) => _positionRepository.GetClosedPositions(start, end);

    public Task<IEnumerable<Position>> GetOpenedPositions() => _positionRepository.GetOpenedPositions();
}
