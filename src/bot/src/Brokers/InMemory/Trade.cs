using bot.src.Brokers;
using bot.src.Brokers.InMemory.Exceptions;
using bot.src.Data;
using bot.src.Data.Models;

namespace bot.src.Broker.InMemory;

public class Trade : ITrade
{
    private readonly IAccount _account;
    private readonly ICandleRepository _candleRepository;
    private readonly IPositionRepository _positionRepository;
    public string Symbol { get; set; }
    private Candle _currentCandle = null!;
    private decimal _leverage;

    public Trade(IAccount account, ICandleRepository candleRepository, IPositionRepository positionRepository, string symbol)
    {
        _account = account;
        _candleRepository = candleRepository;
        _positionRepository = positionRepository;
        Symbol = symbol;
    }

    public async Task CloseAllPositions()
    {
        IEnumerable<Position> openPositions = await GetOpenPositions();

        foreach (Position position in openPositions)
            await ClosePosition(position.Id);
    }

    public async Task ClosePosition(string id)
    {
        Position position = await GetPosition(id) ?? throw new PositionNotFoundException();

        if (position.PositionDirection == PositionDirection.LONG)
            if (_currentCandle.Low <= position.SLPrice)
                position.ClosedPrice = position.SLPrice;
            else if (position.TPPrice != null && _currentCandle.High >= position.TPPrice)
                position.ClosedPrice = position.TPPrice;
            else if (position.TPPrice == null)
                position.ClosedPrice = _currentCandle.Close;
            else
                throw new ClosePositionException();
        else if (position.PositionDirection == PositionDirection.SHORT)
            if (_currentCandle.High >= position.SLPrice)
                position.ClosedPrice = position.SLPrice;
            else if (position.TPPrice != null && _currentCandle.Low <= position.TPPrice)
                position.ClosedPrice = position.TPPrice;
            else if (position.TPPrice == null)
                position.ClosedPrice = _currentCandle.Close;
            else
                throw new ClosePositionException();
        else
            throw new ClosePositionException();

        position.ClosedAt = _currentCandle.Date.AddSeconds(await _candleRepository.GetTimeFrame());
        decimal? profit = (position.ClosedPrice - position.OpenedPrice) * position.Margin * position.Leverage / position.OpenedPrice;
        if (position.PositionDirection == PositionDirection.SHORT)
            profit *= -1;
        position.Profit = profit;
        decimal commission = (decimal)(position.Commission * position.Margin * position.Leverage)!;
        position.Commission = commission;
        position.ProfitWithCommission = profit - commission;

        await _positionRepository.ReplacePosition(position);
    }

    public async Task<IEnumerable<Position>> GetAllPositions() => await _positionRepository.GetPositions();

    public async Task<IEnumerable<Position>> GetAllPositions(DateTime start, DateTime? end = null) => await _positionRepository.GetPositions(start, end);

    public async Task<IEnumerable<Position>> GetClosedPositions() => await _positionRepository.GetClosedPositions();

    public async Task<IEnumerable<Position>> GetClosedPositions(DateTime start, DateTime? end = null) => await _positionRepository.GetClosedPositions(start, end);

    public Task<decimal> GetLeverage() => Task.FromResult(_leverage);

    public async Task<IEnumerable<Position>> GetOpenPositions() => await _positionRepository.GetPositions();

    public async Task<IEnumerable<Position>> GetOpenPositions(DateTime start, DateTime? end = null) => await _positionRepository.GetOpenedPositions(start, end);

    public async Task<Position?> GetPosition(string id) => await _positionRepository.GetPosition(id);

    public Task<decimal> GetPrice() => Task.FromResult(_currentCandle.Close);

    public async Task OpenMarketOrder(decimal margin, bool direction, decimal slPrice, decimal tpPrice)
    {
        if ((await _account.GetBalance()) < margin)
            throw new NotEnoughEquityException();

        await _positionRepository.CreatePosition(_currentCandle.Close, margin, _leverage, slPrice, tpPrice, _currentCandle.Date.AddSeconds(await _candleRepository.GetTimeFrame()));
    }

    public async Task OpenMarketOrder(decimal margin, bool direction, decimal slPrice)
    {
        if ((await _account.GetBalance()) < margin)
            throw new NotEnoughEquityException();

        await _positionRepository.CreatePosition(_currentCandle.Close, margin, _leverage, slPrice, _currentCandle.Date.AddSeconds(await _candleRepository.GetTimeFrame()));
    }

    public Task SetLeverage(decimal leverage)
    {
        _leverage = leverage;
        return Task.CompletedTask;
    }

    public Task<string> GetSymbol() => Task.FromResult(Symbol);

    public Task SetSymbol(string symbol)
    {
        Symbol = symbol;

        return Task.CompletedTask;
    }
}
