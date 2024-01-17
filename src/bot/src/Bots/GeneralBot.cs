using bot.src.Brokers;
using bot.src.Strategies;
using bot.src.Util;
using Serilog;

namespace bot.src.Bots;

public class GeneralBot : IBot
{
    private readonly IStrategy _strategy;
    private readonly IBroker _broker;
    private readonly ILogger _logger;
    private readonly ITime _time;
    private int TimeFrame = 60;

    public GeneralBot(IStrategy strategy, IBroker broker, ITime time, ILogger logger)
    {
        _strategy = strategy;
        _logger = logger.ForContext<GeneralBot>();
        _time = time;
        _broker = broker;
    }

    public async Task Run()
    {
        try
        {
            _logger.Information("Bot started at: {dateTime}", DateTime.UtcNow.ToString());

            await _time.StartTimer(TimeFrame, async (o, args) => await Tick());

            while (true)
            {
                string? userInput = System.Console.ReadLine();

                if (userInput is not null && userInput.ToLower() == "exit")
                    return;
            }
        }
        finally
        {
            _logger.Information("Bot ended at: {dateTime}", DateTime.UtcNow.ToString());
        }
    }

    public async Task Tick()
    {
        _logger.Information("Ticking at: {dateTime}", DateTime.UtcNow.ToString());

        if (!await _strategy.CheckForSignal())
        {
            _logger.Information("No signal");
            return;
        }

        TimeFrame = _strategy.GetTimeFrame();

        if ((!_strategy.ShouldOpenPosition() && !_strategy.ShouldCloseAllPositions()) || (_strategy.ShouldOpenPosition() && _strategy.ShouldCloseAllPositions()))
        {
            _logger.Information("Invalid signal, ShouldOpenPosition and ShouldCloseAllPositions variables are both true at the same time!");
            return;
        }

        if (_strategy.ShouldCloseAllPositions())
        {
            _logger.Information("Closing all of the open positions...");
            await _broker.CloseAllPositions();
            _logger.Information("open positions are closed.");
            return;
        }

        if (!_strategy.IsParallelPositionsAllowed() && (await _broker.GetOpenPositions()).Any())
        {
            _logger.Information("Parallel positions is not allowed, skip until the position is closed.");
            return;
        }

        _logger.Information("Opening a market position...");

        if (_strategy.GetTPPrice() is null)
            await _broker.OpenMarketPosition(_strategy.GetMargin(), _strategy.GetLeverage(), _strategy.GetDirection(), _strategy.GetSLPrice());
        else
            await _broker.OpenMarketPosition(_strategy.GetMargin(), _strategy.GetLeverage(), _strategy.GetDirection(), _strategy.GetSLPrice(), (decimal)_strategy.GetTPPrice()!);

        _logger.Information("market position is opened.");
    }
}
