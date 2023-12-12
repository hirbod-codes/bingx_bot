using bot.src;
using broker_api.src;
using email_api.src;
using Moq;

namespace TestBots;

public class TestGeneralBot
{
    private IBot Bot { get; set; } = null!;

    [Fact]
    public void TestRunWithTP()
    {
        Mock<IStrategy> strategy = new();
        Mock<IAccount> account = new();
        Mock<ITrade> trade = new();
        Mock<IMarket> market = new();
        Mock<IUtilities> utilities = new();
        Mock<IBingxUtilities> bingxUtilities = new();
        Mock<ISignalProvider> lastSignal = new();
        int timeFrame = 1;
        int leverage = 50;
        bool isLong = true;
        float margin = 100;
        float lastPrice = 39876;
        float sl = 39876 - 100;
        float? tp = 39876 + 100;

        lastSignal.Setup(o => o.IsSignalLong()).Returns(value: isLong);
        lastSignal.Setup(o => o.GetTPPrice()).Returns(value: tp);
        lastSignal.Setup(o => o.GetSLPrice()).Returns(value: sl);
        lastSignal.Setup(o => o.GetMargin()).Returns(value: margin);
        lastSignal.Setup(o => o.GetLeverage()).Returns(value: leverage);

        trade.Setup(o => o.GetSymbol()).Returns("symbol");

        Bot = new Bot(strategy.Object, account.Object, trade.Object, market.Object, timeFrame, bingxUtilities.Object, utilities.Object);

        DateTime start = DateTime.UtcNow;
        DateTime? terminationDate = start.AddMinutes(11);

        strategy.Setup(o => o.Initiate());

        utilities.Setup(o => o.IsTerminationDatePassed(terminationDate)).Returns(false);
        utilities.Setup(o => o.HasTimeFrameReached(timeFrame)).Returns(false);
        utilities.Setup(o => o.Sleep(1000));

        utilities.Setup(o => o.IsTerminationDatePassed(terminationDate)).Returns(false);
        utilities.Setup(o => o.HasTimeFrameReached(timeFrame)).Returns(true);
        bingxUtilities.Setup(o => o.NotifyListeners("Candle Created."));
        utilities.Setup(o => o.Sleep(2500));
        strategy.Setup(o => o.CheckOpenPositionSignal(null)).Returns(Task.FromResult(false));

        utilities.Setup(o => o.IsTerminationDatePassed(terminationDate)).Returns(false);
        utilities.Setup(o => o.HasTimeFrameReached(timeFrame)).Returns(true);
        bingxUtilities.Setup(o => o.NotifyListeners("Candle Created."));
        utilities.Setup(o => o.Sleep(2500));
        strategy.Setup(o => o.CheckOpenPositionSignal(null)).Returns(Task.FromResult(true));
        market.Setup(o => o.GetLastPrice("symbol", timeFrame)).Returns(Task.FromResult(lastPrice));
        strategy.Setup(o => o.GetLastSignal()).Returns(lastSignal.Object);
        trade.Setup(o => o.SetLeverage(leverage, true)).Returns(Task.FromResult(new HttpResponseMessage()));
        bingxUtilities.Setup(o => o.EnsureSuccessfulBingxResponse(It.IsAny<HttpResponseMessage>()));
        trade.Setup(o => o.SetLeverage(leverage, false)).Returns(Task.FromResult(new HttpResponseMessage()));
        bingxUtilities.Setup(o => o.EnsureSuccessfulBingxResponse(It.IsAny<HttpResponseMessage>()));
        trade.Setup(o => o.OpenMarketOrder(isLong, (float)(margin * leverage / lastPrice), tp, sl)).Returns(Task.FromResult(new HttpResponseMessage()));
        bingxUtilities.Setup(o => o.EnsureSuccessfulBingxResponse(It.IsAny<HttpResponseMessage>()));

        utilities.Setup(o => o.IsTerminationDatePassed(terminationDate)).Returns(false);
        utilities.Setup(o => o.HasTimeFrameReached(timeFrame)).Returns(true);
        bingxUtilities.Setup(o => o.NotifyListeners("Candle Created."));
        utilities.Setup(o => o.Sleep(2500));
        account.Setup(o => o.GetOpenPositionCount()).Returns(Task.FromResult(1));

        utilities.Setup(o => o.IsTerminationDatePassed(terminationDate)).Returns(false);
        utilities.Setup(o => o.HasTimeFrameReached(timeFrame)).Returns(true);
        bingxUtilities.Setup(o => o.NotifyListeners("Candle Created."));
        utilities.Setup(o => o.Sleep(2500));
        account.Setup(o => o.GetOpenPositionCount()).Returns(Task.FromResult(0));
        strategy.Setup(o => o.CheckOpenPositionSignal(isLong)).Returns(Task.FromResult(false));

        utilities.Setup(o => o.IsTerminationDatePassed(terminationDate)).Returns(false);
        utilities.Setup(o => o.HasTimeFrameReached(timeFrame)).Returns(true);
        bingxUtilities.Setup(o => o.NotifyListeners("Candle Created."));
        utilities.Setup(o => o.Sleep(2500));
        account.Setup(o => o.GetOpenPositionCount()).Returns(Task.FromResult(0));
        strategy.Setup(o => o.CheckOpenPositionSignal(isLong)).Returns(Task.FromResult(true));

        isLong = !isLong;
        lastSignal.Setup(o => o.IsSignalLong()).Returns(value: isLong);

        market.Setup(o => o.GetLastPrice("symbol", timeFrame)).Returns(Task.FromResult(lastPrice));
        strategy.Setup(o => o.GetLastSignal()).Returns(lastSignal.Object);
        trade.Setup(o => o.SetLeverage(leverage, true)).Returns(Task.FromResult(new HttpResponseMessage()));
        bingxUtilities.Setup(o => o.EnsureSuccessfulBingxResponse(It.IsAny<HttpResponseMessage>()));
        trade.Setup(o => o.SetLeverage(leverage, false)).Returns(Task.FromResult(new HttpResponseMessage()));
        bingxUtilities.Setup(o => o.EnsureSuccessfulBingxResponse(It.IsAny<HttpResponseMessage>()));
        trade.Setup(o => o.OpenMarketOrder(isLong, (float)(margin * leverage / lastPrice), tp, sl)).Returns(Task.FromResult(new HttpResponseMessage()));
        bingxUtilities.Setup(o => o.EnsureSuccessfulBingxResponse(It.IsAny<HttpResponseMessage>()));

        utilities.Setup(o => o.IsTerminationDatePassed(terminationDate)).Returns(false);
        utilities.Setup(o => o.HasTimeFrameReached(timeFrame)).Returns(true);
        bingxUtilities.Setup(o => o.NotifyListeners("Candle Created."));
        utilities.Setup(o => o.Sleep(2500));
        account.Setup(o => o.GetOpenPositionCount()).Returns(Task.FromResult(1));

        utilities.Setup(o => o.IsTerminationDatePassed(terminationDate)).Returns(false);
        utilities.Setup(o => o.HasTimeFrameReached(timeFrame)).Returns(true);
        bingxUtilities.Setup(o => o.NotifyListeners("Candle Created."));
        utilities.Setup(o => o.Sleep(2500));
        account.Setup(o => o.GetOpenPositionCount()).Returns(Task.FromResult(0));
        strategy.Setup(o => o.CheckOpenPositionSignal(isLong)).Returns(Task.FromResult(false));

        utilities.Setup(o => o.IsTerminationDatePassed(terminationDate)).Returns(true);

        Bot.Run(terminationDate);
    }

    [Fact]
    public void TestRunWithoutTP()
    {
        Mock<IStrategy> strategy = new();
        Mock<IAccount> account = new();
        Mock<ITrade> trade = new();
        Mock<IMarket> market = new();
        Mock<IUtilities> utilities = new();
        Mock<IBingxUtilities> bingxUtilities = new();
        Mock<ISignalProvider> lastSignal = new();
        int timeFrame = 1;
        int leverage = 50;
        bool isLong = true;
        float margin = 100;
        float lastPrice = 39876;
        float sl = 38467;
        float? tp = null;

        lastSignal.Setup(o => o.IsSignalLong()).Returns(value: isLong);
        lastSignal.Setup(o => o.GetTPPrice()).Returns(value: tp);
        lastSignal.Setup(o => o.GetSLPrice()).Returns(value: sl);
        lastSignal.Setup(o => o.GetMargin()).Returns(value: margin);
        lastSignal.Setup(o => o.GetLeverage()).Returns(value: leverage);

        trade.Setup(o => o.GetSymbol()).Returns("symbol");

        Bot = new Bot(strategy.Object, account.Object, trade.Object, market.Object, timeFrame, bingxUtilities.Object, utilities.Object);

        DateTime start = DateTime.UtcNow;
        DateTime? terminationDate = start.AddMinutes(11);

        strategy.Setup(o => o.Initiate());

        utilities.Setup(o => o.IsTerminationDatePassed(terminationDate)).Returns(false);
        utilities.Setup(o => o.HasTimeFrameReached(timeFrame)).Returns(false);
        utilities.Setup(o => o.Sleep(1000));

        utilities.Setup(o => o.IsTerminationDatePassed(terminationDate)).Returns(false);
        utilities.Setup(o => o.HasTimeFrameReached(timeFrame)).Returns(true);
        bingxUtilities.Setup(o => o.NotifyListeners("Candle Created."));
        utilities.Setup(o => o.Sleep(2500));
        strategy.Setup(o => o.CheckOpenPositionSignal(null)).Returns(Task.FromResult(false));

        utilities.Setup(o => o.IsTerminationDatePassed(terminationDate)).Returns(false);
        utilities.Setup(o => o.HasTimeFrameReached(timeFrame)).Returns(true);
        bingxUtilities.Setup(o => o.NotifyListeners("Candle Created."));
        utilities.Setup(o => o.Sleep(2500));
        strategy.Setup(o => o.CheckOpenPositionSignal(null)).Returns(Task.FromResult(true));
        market.Setup(o => o.GetLastPrice("symbol", timeFrame)).Returns(Task.FromResult(lastPrice));
        strategy.Setup(o => o.GetLastSignal()).Returns(lastSignal.Object);
        trade.Setup(o => o.SetLeverage(leverage, true)).Returns(Task.FromResult(new HttpResponseMessage()));
        bingxUtilities.Setup(o => o.EnsureSuccessfulBingxResponse(It.IsAny<HttpResponseMessage>()));
        trade.Setup(o => o.SetLeverage(leverage, false)).Returns(Task.FromResult(new HttpResponseMessage()));
        bingxUtilities.Setup(o => o.EnsureSuccessfulBingxResponse(It.IsAny<HttpResponseMessage>()));
        trade.Setup(o => o.OpenMarketOrder(isLong, (float)(margin * leverage / lastPrice), tp, sl)).Returns(Task.FromResult(new HttpResponseMessage()));
        bingxUtilities.Setup(o => o.EnsureSuccessfulBingxResponse(It.IsAny<HttpResponseMessage>()));

        utilities.Setup(o => o.IsTerminationDatePassed(terminationDate)).Returns(false);
        utilities.Setup(o => o.HasTimeFrameReached(timeFrame)).Returns(true);
        bingxUtilities.Setup(o => o.NotifyListeners("Candle Created."));
        utilities.Setup(o => o.Sleep(2500));
        strategy.Setup(o => o.CheckClosePositionSignal(isLong)).Returns(Task.FromResult(false));

        utilities.Setup(o => o.IsTerminationDatePassed(terminationDate)).Returns(false);
        utilities.Setup(o => o.HasTimeFrameReached(timeFrame)).Returns(true);
        bingxUtilities.Setup(o => o.NotifyListeners("Candle Created."));
        utilities.Setup(o => o.Sleep(2500));
        strategy.Setup(o => o.CheckClosePositionSignal(isLong)).Returns(Task.FromResult(true));
        market.Setup(o => o.GetLastPrice("symbol", timeFrame)).Returns(Task.FromResult(lastPrice));
        strategy.Setup(o => o.GetLastSignal()).Returns(lastSignal.Object);
        lastSignal.Setup(o => o.GetTPPrice()).Returns(value: tp);
        trade.Setup(o => o.CloseOpenPositions());
        bingxUtilities.Setup(o => o.EnsureSuccessfulBingxResponse(It.IsAny<HttpResponseMessage>()));
        bingxUtilities.Setup(o => o.CalculateFinancialPerformance(It.IsAny<DateTime>(), trade.Object));

        utilities.Setup(o => o.IsTerminationDatePassed(terminationDate)).Returns(false);
        utilities.Setup(o => o.HasTimeFrameReached(timeFrame)).Returns(true);
        bingxUtilities.Setup(o => o.NotifyListeners("Candle Created."));
        utilities.Setup(o => o.Sleep(2500));
        strategy.Setup(o => o.CheckOpenPositionSignal(isLong)).Returns(Task.FromResult(false));

        utilities.Setup(o => o.IsTerminationDatePassed(terminationDate)).Returns(false);
        utilities.Setup(o => o.HasTimeFrameReached(timeFrame)).Returns(true);
        bingxUtilities.Setup(o => o.NotifyListeners("Candle Created."));
        utilities.Setup(o => o.Sleep(2500));
        strategy.Setup(o => o.CheckOpenPositionSignal(isLong)).Returns(Task.FromResult(true));

        isLong = !isLong;
        lastSignal.Setup(o => o.IsSignalLong()).Returns(value: isLong);

        market.Setup(o => o.GetLastPrice("symbol", timeFrame)).Returns(Task.FromResult(lastPrice));
        strategy.Setup(o => o.GetLastSignal()).Returns(lastSignal.Object);
        trade.Setup(o => o.SetLeverage(leverage, true)).Returns(Task.FromResult(new HttpResponseMessage()));
        bingxUtilities.Setup(o => o.EnsureSuccessfulBingxResponse(It.IsAny<HttpResponseMessage>()));
        trade.Setup(o => o.SetLeverage(leverage, false)).Returns(Task.FromResult(new HttpResponseMessage()));
        bingxUtilities.Setup(o => o.EnsureSuccessfulBingxResponse(It.IsAny<HttpResponseMessage>()));
        trade.Setup(o => o.OpenMarketOrder(isLong, (float)(margin * leverage / lastPrice), tp, sl)).Returns(Task.FromResult(new HttpResponseMessage()));
        bingxUtilities.Setup(o => o.EnsureSuccessfulBingxResponse(It.IsAny<HttpResponseMessage>()));

        utilities.Setup(o => o.IsTerminationDatePassed(terminationDate)).Returns(false);
        utilities.Setup(o => o.HasTimeFrameReached(timeFrame)).Returns(true);
        bingxUtilities.Setup(o => o.NotifyListeners("Candle Created."));
        utilities.Setup(o => o.Sleep(2500));
        strategy.Setup(o => o.CheckClosePositionSignal(isLong)).Returns(Task.FromResult(true));
        market.Setup(o => o.GetLastPrice("symbol", timeFrame)).Returns(Task.FromResult(lastPrice));
        strategy.Setup(o => o.GetLastSignal()).Returns(lastSignal.Object);
        lastSignal.Setup(o => o.GetTPPrice()).Returns(value: tp);
        trade.Setup(o => o.CloseOpenPositions());
        bingxUtilities.Setup(o => o.EnsureSuccessfulBingxResponse(It.IsAny<HttpResponseMessage>()));
        bingxUtilities.Setup(o => o.CalculateFinancialPerformance(It.IsAny<DateTime>(), trade.Object));

        utilities.Setup(o => o.IsTerminationDatePassed(terminationDate)).Returns(false);
        utilities.Setup(o => o.HasTimeFrameReached(timeFrame)).Returns(true);
        bingxUtilities.Setup(o => o.NotifyListeners("Candle Created."));
        utilities.Setup(o => o.Sleep(2500));
        strategy.Setup(o => o.CheckOpenPositionSignal(isLong)).Returns(Task.FromResult(false));

        utilities.Setup(o => o.IsTerminationDatePassed(terminationDate)).Returns(true);

        Bot.Run(terminationDate);
    }
}
