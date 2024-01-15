using bot.src.Brokers;
using bot.src.Data;
using bot.src.Data.Models;

namespace bot.src.Broker.InMemory;

public class Broker : IBroker
{
    public event EventHandler? CandleProcessed;
    private readonly ICandleRepository _candleRepository;
    private readonly ITrade _trade;
    private Candle _currentCandle = null!;

    public Broker(ITrade trade, ICandleRepository candleRepository)
    {
        _trade = trade;
        _candleRepository = candleRepository;
    }

    private void OnCandleProcessed() => CandleProcessed?.Invoke(this, EventArgs.Empty);

    public async Task CandleClosed(Candle candle)
    {
        IEnumerable<Position> openPositions = await _trade.GetOpenPositions();
        _currentCandle = candle;

        foreach (Position position in openPositions)
            if (await ShouldClosePosition(position))
                await _trade.ClosePosition(position.Id);

        OnCandleProcessed();
    }

    public async Task CandleClosed(int index)
    {
        IEnumerable<Position> openPositions = await _trade.GetOpenPositions();
        _currentCandle = await _candleRepository.GetCandle(index);

        foreach (Position position in openPositions)
            if (await ShouldClosePosition(position))
                await _trade.ClosePosition(position.Id);
    }

    private Task<bool> ShouldClosePosition(Position position) => Task.FromResult((position.PositionDirection == PositionDirection.LONG && (_currentCandle.Low <= position.SLPrice || (position.TPPrice != null && _currentCandle.High >= position.TPPrice))) || (position.PositionDirection == PositionDirection.SHORT && (_currentCandle.High >= position.SLPrice || (position.TPPrice != null && _currentCandle.Low <= position.TPPrice))));
}
