using bot.src.Brokers.InMemory.Exceptions;
using bot.src.Data;
using bot.src.Data.Models;
using Serilog;

namespace bot.src.Brokers.InMemory;

public class Broker : IBroker
{
    private readonly ICandleRepository _candleRepository;
    private readonly IAccount _account;
    private readonly ITrade _trade;
    private readonly ILogger _logger;
    private readonly BrokerOptions _brokerOptions;
    private Candle _currentCandle = null!;

    public Broker(BrokerOptions brokerOptions, ITrade trade, IAccount account, ICandleRepository candleRepository, ILogger logger)
    {
        _trade = trade;
        _candleRepository = candleRepository;
        _logger = logger.ForContext<Broker>();
        _brokerOptions = brokerOptions;
        _account = account;
    }

    public async Task CandleClosed(Candle candle)
    {
        _currentCandle = candle;

        _logger.Information("Getting open positions...");
        IEnumerable<Position> openPositions = await _trade.GetOpenPositions();

        if (!openPositions.Any())
        {
            _logger.Information("There is no open position.");
            return;
        }

        _logger.Information("Closing open positions that suppose to be closed.");
        int closedPositionsCount = 0;
        foreach (Position position in openPositions)
            if (ShouldClosePosition(position))
            {
                closedPositionsCount++;
                await ClosePosition(position);
            }
        _logger.Information("{closedPositionsCount} positions closed.", closedPositionsCount);
    }

    private bool ShouldClosePosition(Position position) =>
        (
            position.PositionDirection == PositionDirection.LONG &&
            (_currentCandle.Low <= position.SLPrice || (position.TPPrice != null && _currentCandle.High >= position.TPPrice))
        ) ||
        (
            position.PositionDirection == PositionDirection.SHORT &&
            (_currentCandle.High >= position.SLPrice || (position.TPPrice != null && _currentCandle.Low <= position.TPPrice))
        );

    private async Task ClosePosition(Position position)
    {
        decimal? closedPrice = null!;
        if (position.PositionDirection == PositionDirection.LONG)
        {
            if (_currentCandle.Low <= position.SLPrice)
                closedPrice = position.SLPrice;
            else if (position.TPPrice != null && _currentCandle.High >= position.TPPrice)
                closedPrice = (decimal)position.TPPrice;
        }
        else if (position.PositionDirection == PositionDirection.SHORT)
        {
            if (_currentCandle.High >= position.SLPrice)
                closedPrice = position.SLPrice;
            else if (position.TPPrice != null && _currentCandle.Low <= position.TPPrice)
                closedPrice = (decimal)position.TPPrice;
        }
        else
            throw new ClosePositionException();

        await _trade.ClosePosition(position.Id, (decimal)closedPrice, _currentCandle.Date.AddSeconds(await _candleRepository.GetTimeFrame()));
    }

    public async Task CandleClosed(int index) => await CandleClosed(await _candleRepository.GetCandle(index));

    public Candle GetCurrentCandle() => _currentCandle;

    public async Task CloseAllPositions() => await _trade.CloseAllPositions(_currentCandle);

    public async Task<IEnumerable<Position>> GetOpenPositions() => await _trade.GetOpenPositions();

    public async Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice, decimal tpPrice) => await _trade.OpenMarketPosition(new Position()
    {
        Leverage = leverage,
        Margin = margin,
        OpenedAt = _currentCandle.Date.AddSeconds(await _candleRepository.GetTimeFrame()),
        OpenedPrice = _currentCandle.Close,
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
        OpenedAt = _currentCandle.Date.AddSeconds(await _candleRepository.GetTimeFrame()),
        OpenedPrice = _currentCandle.Close,
        SLPrice = slPrice,
        CommissionRatio = _brokerOptions.BrokerCommission,
        Symbol = _brokerOptions.Symbol,
        PositionDirection = direction,
    });
}
