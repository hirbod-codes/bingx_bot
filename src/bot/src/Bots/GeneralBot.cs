using bot.src.Brokers;
using bot.src.Strategies;
using bot.src.Util;
using Serilog;

namespace bot.src.Bots;

public class GeneralBot : IBot
{
    private readonly IStrategy Strategy;
    private readonly ITrade Trade;
    private readonly ILogger Logger;
    private readonly ITime Time;
    private int TimeFrame = 60;

    public GeneralBot(IStrategy strategy, ITrade trade, ITime time, ILogger logger)
    {
        Strategy = strategy;
        Logger = logger.ForContext<GeneralBot>();
        Time = time;
        Trade = trade;
    }

    public async Task Run()
    {
        try
        {
            Logger.Information("Bot started at: {dateTime}", DateTime.UtcNow.ToString());

            await Time.StartTimer(TimeFrame, async (o, args) => await Tick());

            while (true)
            {
                string? userInput = System.Console.ReadLine();

                if (userInput is not null && userInput.ToLower() == "exit")
                    return;
            }
        }
        finally
        {
            Logger.Information("Bot ended at: {dateTime}", DateTime.UtcNow.ToString());
        }
    }

    public async Task Tick()
    {
        Logger.Information("Ticked at: {dateTime}", DateTime.UtcNow.ToString());

        if (!await Strategy.CheckForSignal())
        {
            Logger.Information("No signal");
            return;
        }

        TimeFrame = Strategy.GetTimeFrame();

        if ((!Strategy.ShouldOpenPosition() && !Strategy.ShouldCloseAllPositions()) || (Strategy.ShouldOpenPosition() && Strategy.ShouldCloseAllPositions()))
        {
            Logger.Information("Invalid signal, ShouldOpenPosition and ShouldCloseAllPositions variables are both true at the same time!");
            return;
        }

        if (Strategy.ShouldCloseAllPositions())
        {
            Logger.Information("Closing all of the open positions...");
            await Trade.CloseAllPositions();
            Logger.Information("open positions are closed.");
            return;
        }

        if (!Strategy.IsParallelPositionsAllowed() && (await Trade.GetOpenPositions()).Any())
        {
            Logger.Information("Parallel positions is not allowed, skip until the position is closed.");
            return;
        }

        Logger.Information("Opening a market position...");

        await Trade.SetLeverage(Strategy.GetLeverage());

        if (Strategy.GetTPPrice() is null)
            await Trade.OpenMarketOrder(Strategy.GetMargin(), Strategy.GetDirection(), Strategy.GetSLPrice());
        else
            await Trade.OpenMarketOrder(Strategy.GetMargin(), Strategy.GetDirection(), Strategy.GetSLPrice(), (decimal)Strategy.GetTPPrice()!);

        Logger.Information("market position is opened.");
    }
}
