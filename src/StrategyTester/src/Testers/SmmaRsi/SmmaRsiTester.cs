using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Data;
using bot.src.Data.Models;
using bot.src.Strategies;
using StrategyTester.src.Utils;

namespace StrategyTester.src.Testers.SmmaRsi;

public class SmmaRsiTester : ITester
{
    private readonly ICandleRepository _candleRepository;
    private readonly ITime _time;
    private readonly IStrategy _strategy;
    private readonly IBroker _broker;
    private readonly IBot _bot;

    public SmmaRsiTester(ICandleRepository candleRepository, ITime time, IStrategy strategy, IBroker broker, IBot bot)
    {
        _candleRepository = candleRepository;
        _time = time;
        _strategy = strategy;
        _broker = broker;
        _bot = bot;
    }

    public async Task Test()
    {
        Candles candles = await _candleRepository.GetCandles();

        for (int i = candles.Count() - 1; i > -1; i--)
        {
            Candle candle = candles.ElementAt(i);

            await _broker.SetCurrentCandle(candle);

            _time.SetUtcNow(candle.Date);

            await _strategy.HandleCandle(candle, i);

            await _broker.CandleClosed();
            await _bot.Tick();
        }
    }
}
