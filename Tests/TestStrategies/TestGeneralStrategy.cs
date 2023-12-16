using email_api.src;
using Moq;
using Serilog;

namespace TestStrategies;

public class TestGeneralStrategy
{
    [Fact]
    public async Task TestCheckClosePosition()
    {
        Mock<ISignalProvider> signalProvider = new();
        Mock<ILogger> logger = new();
        int timeFrame = 1;
        bool? isLastOpenPositionLong = null;
        bool? result = null;

        GeneralStrategy generalStrategy = new(signalProvider.Object, logger.Object);

        result = await generalStrategy.CheckClosePositionSignal(isLastOpenPositionLong, timeFrame);
        Assert.False(result);

        isLastOpenPositionLong = false;
        signalProvider.Setup(o => o.ResetSignals());
        signalProvider.Setup(o => o.CheckSignals()).Returns(Task.FromResult(false));
        result = await generalStrategy.CheckClosePositionSignal(isLastOpenPositionLong, timeFrame);
        Assert.False(result);

        isLastOpenPositionLong = false;
        signalProvider.Setup(o => o.ResetSignals());
        signalProvider.Setup(o => o.CheckSignals()).Returns(Task.FromResult(true));
        signalProvider.Setup(o => o.GetSignalTime()).Returns(DateTime.UtcNow.AddMinutes(-2));
        result = await generalStrategy.CheckClosePositionSignal(isLastOpenPositionLong, timeFrame);
        Assert.False(result);

        isLastOpenPositionLong = false;
        signalProvider.Setup(o => o.ResetSignals());
        signalProvider.Setup(o => o.CheckSignals()).Returns(Task.FromResult(true));
        signalProvider.Setup(o => o.GetSignalTime()).Returns(DateTime.UtcNow.AddSeconds(-20));
        signalProvider.Setup(o => o.IsSignalLong()).Returns(false);
        result = await generalStrategy.CheckClosePositionSignal(isLastOpenPositionLong, timeFrame);
        Assert.False(result);

        isLastOpenPositionLong = false;
        signalProvider.Setup(o => o.ResetSignals());
        signalProvider.Setup(o => o.CheckSignals()).Returns(Task.FromResult(true));
        signalProvider.Setup(o => o.GetSignalTime()).Returns(DateTime.UtcNow.AddSeconds(-20));
        signalProvider.Setup(o => o.IsSignalLong()).Returns(true);
        result = await generalStrategy.CheckClosePositionSignal(isLastOpenPositionLong, timeFrame);
        Assert.True(result);
    }

    [Fact]
    public async Task TestCheckOpenPosition()
    {
        Mock<ISignalProvider> signalProvider = new();
        Mock<ILogger> logger = new();
        int timeFrame = 1;
        bool? isLastOpenPositionLong = null;
        bool? result = null;

        GeneralStrategy generalStrategy = new(signalProvider.Object, logger.Object);

        isLastOpenPositionLong = true;
        result = await generalStrategy.CheckOpenPositionSignal(isLastOpenPositionLong, timeFrame);
        Assert.False(result);

        isLastOpenPositionLong = false;
        result = await generalStrategy.CheckOpenPositionSignal(isLastOpenPositionLong, timeFrame);
        Assert.False(result);

        isLastOpenPositionLong = null;
        signalProvider.Setup(o => o.ResetSignals());
        signalProvider.Setup(o => o.CheckSignals()).Returns(Task.FromResult(false));
        result = await generalStrategy.CheckOpenPositionSignal(isLastOpenPositionLong, timeFrame);
        Assert.False(result);

        isLastOpenPositionLong = null;
        signalProvider.Setup(o => o.ResetSignals());
        signalProvider.Setup(o => o.CheckSignals()).Returns(Task.FromResult(true));
        signalProvider.Setup(o => o.GetSignalTime()).Returns(DateTime.UtcNow.AddMinutes(-2));
        result = await generalStrategy.CheckOpenPositionSignal(isLastOpenPositionLong, timeFrame);
        Assert.False(result);

        isLastOpenPositionLong = null;
        signalProvider.Setup(o => o.ResetSignals());
        signalProvider.Setup(o => o.CheckSignals()).Returns(Task.FromResult(true));
        signalProvider.Setup(o => o.GetSignalTime()).Returns(DateTime.UtcNow.AddSeconds(-20));
        result = await generalStrategy.CheckOpenPositionSignal(isLastOpenPositionLong, timeFrame);
        Assert.True(result);
    }

    [Fact]
    public void TestGetLastSignal()
    {
        Mock<ISignalProvider> signalProvider = new();
        Mock<ILogger> logger = new();
        ISignalProvider? lastSignalProvider = null;

        GeneralStrategy generalStrategy = new(signalProvider.Object, logger.Object);

        lastSignalProvider = generalStrategy.GetLastSignal();
        Assert.Equal(signalProvider.Object, lastSignalProvider);
    }

    [Fact]
    public async Task TestInitiate()
    {
        Mock<ISignalProvider> signalProvider = new();
        Mock<ILogger> logger = new();

        GeneralStrategy generalStrategy = new(signalProvider.Object, logger.Object);

        signalProvider.Setup(o => o.Initiate());
        await generalStrategy.Initiate();
    }
}
