using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Data;
using bot.src.Strategies;
using StrategyTester.src.Utils;
using StrategyTester.src.Testers.SmmaRsi;

namespace StrategyTester.src.Testers;

public static class TesterFactory
{
    public static ITester CreateTester(string testerName, IPositionRepository positionRepository, ICandleRepository candleRepository, ITime time, IStrategy strategy, IBroker broker, IBot bot) => testerName switch
    {
        "SmmaRsi" => new SmmaRsiTester(positionRepository, candleRepository, time, strategy, broker, bot),
        _ => throw new Exception()
    };
}
