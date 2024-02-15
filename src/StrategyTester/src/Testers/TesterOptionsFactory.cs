using GeneralTesterOptions = StrategyTester.src.Testers.General.TesterOptions;

namespace StrategyTester.src.Testers;

public static class TesterOptionsFactory
{
    public static ITesterOptions CreateTesterOptions(string testerName) => testerName switch
    {
        TestersNames.GENERAL => new GeneralTesterOptions(),
        _ => throw new TesterException()
    };
}
