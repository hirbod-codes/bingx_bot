// using bot.src.Bots;
// using bot.src.Brokers;
// using bot.src.Data;
// using bot.src.Data.Models;
// using bot.src.Strategies;
// using Serilog;
// using StrategyTester.src.Utils;

// namespace StrategyTester.src.Testers.General;

// public class GeneralTester : ITester
// {
//     private readonly ICandleRepository _candleRepository;
//     private readonly ITime _time;
//     private readonly IStrategy _strategy;
//     private readonly IBroker _broker;
//     private readonly IBot _bot;
//     private readonly ILogger _logger;

//     public GeneralTester(ICandleRepository candleRepository, ITime time, IStrategy strategy, IBroker broker, IBot bot, ILogger logger)
//     {
//         _candleRepository = candleRepository;
//         _time = time;
//         _strategy = strategy;
//         _broker = broker;
//         _bot = bot;
//         _logger = logger.ForContext<GeneralTester>();
//     }

//     public async Task Test()
//     {
//         Candles candles = await _candleRepository.GetCandles();
//         int timeFrame = candles.TimeFrame;

//         _strategy.InitializeIndicators(candles);

//         _logger.Information($"number of candles: {candles.Count()}");

//         for (int i = candles.Count() - 1; i > -1; i--)
//         {
//             _logger.Information($"candle index: {i}");

//             Candle candle = candles.ElementAt(i);

//             _time.SetUtcNow(candle.Date);

//             await _strategy.HandleCandle(candle, i, timeFrame);

//             await _broker.CandleClosed();
//             await _bot.Tick();
//         }
//     }
// }
