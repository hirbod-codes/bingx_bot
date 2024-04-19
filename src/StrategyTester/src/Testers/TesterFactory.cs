using Abstractions.src.Bots;
using Abstractions.src.Brokers;
using Abstractions.src.Strategies;
using Abstractions.src.Utilities;
using StrategyTester.src.Testers.General;
using ILogger = Serilog.ILogger;

namespace StrategyTester.src.Testers;

public static class TesterFactory
{
    public static ITester CreateTester(string testerName, ITesterOptions testerOptions, ITimeSimulator time, IStrategy strategy, IBrokerSimulator broker, IBot bot, ILogger logger) => testerName switch
    {
        TestersNames.GENERAL => new GeneralTester(testerOptions, time, strategy, broker, bot, logger),
        _ => throw new Exception()
    };
}
