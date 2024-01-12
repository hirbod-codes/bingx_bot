using bot.src.MessageStores.Models;
using bot.src.Strategies.GeneralStrategy;
using Serilog;

namespace unit_tests.src.Strategies.GeneralStrategy;

[Collection("Strategy")]
public class GeneralStrategyTest
{
    public const string MESSAGE_DELIMITER = bot.src.Strategies.GeneralStrategy.GeneralStrategy.MESSAGE_DELIMITER;
    public const string FIELD_DELIMITER = bot.src.Strategies.GeneralStrategy.GeneralStrategy.FIELD_DELIMITER;
    public const string KEY_VALUE_PAIR_DELIMITER = bot.src.Strategies.GeneralStrategy.GeneralStrategy.KEY_VALUE_PAIR_DELIMITER;
    private readonly string provider = "provider";
    public Fixture Fixture { get; private set; }

    public GeneralStrategyTest(Fixture fixture) => Fixture = fixture;

    public bot.src.Strategies.GeneralStrategy.GeneralStrategy Instantiate() => new(provider, Fixture.IMessageStore.Object, new LoggerConfiguration().CreateLogger());

    public static readonly List<object[]> TestIsParallelPositionsAllowedData = new(){
        new object[] {"1"},
        new object[] {"0"}
    };

    [Theory]
    [MemberData(nameof(TestIsParallelPositionsAllowedData))]
    public async Task TestIsParallelPositionsAllowed(string allowingParallelPositions)
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}{allowingParallelPositions}{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}100{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}60{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}120" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
        await instance.CheckForSignal();

        Assert.IsType<bool>(instance.IsParallelPositionsAllowed());
    }

    public static readonly List<object[]> TestShouldCloseAllPositionsData = new(){
        new object[] {"0", "1"},
        new object[] {"1", "0"}
    };

    [Theory]
    [MemberData(nameof(TestShouldCloseAllPositionsData))]
    public async Task TestShouldCloseAllPositions(string closingAllPositions, string openingPosition)
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}{closingAllPositions}{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}{openingPosition}{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}100{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}60{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}120" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
        await instance.CheckForSignal();

        Assert.IsType<bool>(instance.ShouldCloseAllPositions());
    }

    public static readonly List<object[]> TestGetDirectionData = new(){
        new object[] {"0"},
        new object[] {"1"},
        new object[] {"1  "},
        new object[] {"  1"},
        new object[] {"  1  "}
    };

    [Theory]
    [MemberData(nameof(TestGetDirectionData))]
    public async Task TestGetDirection(string direction)
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}{direction}{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}100{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}60{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}120" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
        await instance.CheckForSignal();

        Assert.IsType<bool>(instance.GetDirection());
    }

    public static readonly List<object[]> TestGetLeverageData = new(){
        new object[] {"10"},
        new object[] {"10.6"},
        new object[] {"  10.6"},
        new object[] {"10.6"}
    };

    [Theory]
    [MemberData(nameof(TestGetLeverageData))]
    public async Task TestGetLeverage(string leverage)
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}{leverage}{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}100{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}60{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}120" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
        await instance.CheckForSignal();

        Assert.IsType<float>(instance.GetLeverage());
    }

    public static readonly List<object[]> TestGetLeverageFailureData = new(){
        new object[] {"-10.1"},
        new object[] {"0"},
        new object[] {"aa"}
    };

    [Theory]
    [MemberData(nameof(TestGetLeverageFailureData))]
    public async Task TestGetLeverageFailure(string leverage)
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}{leverage}{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}100{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}60{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}120" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

        await Assert.ThrowsAsync<MessageParseException>(instance.CheckForSignal);
    }

    public static readonly List<object[]> TestGetMarginData = new(){
        new object[] {"10"},
        new object[] {"10.6"},
        new object[] {"  10.6"},
        new object[] {"10.6"}
    };

    [Theory]
    [MemberData(nameof(TestGetMarginData))]
    public async Task TestGetMargin(string margin)
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}10.5{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}{margin}{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}100{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}60{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}120" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
        await instance.CheckForSignal();

        Assert.IsType<float>(instance.GetMargin());
    }

    public static readonly List<object[]> TestGetMarginFailureData = new(){
        new object[] {"-10.1"},
        new object[] {"0"},
        new object[] {"aa"}
    };

    [Theory]
    [MemberData(nameof(TestGetMarginFailureData))]
    public async Task TestGetMarginFailure(string margin)
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}10.6{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}{margin}{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}100{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}60{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}120" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

        await Assert.ThrowsAsync<MessageParseException>(instance.CheckForSignal);
    }

    public static readonly List<object[]> TestShouldOpenPositionData = new(){
        new object[] {"0", "1"},
        new object[] {"1", "0"}
    };

    [Theory]
    [MemberData(nameof(TestShouldOpenPositionData))]
    public async Task TestShouldOpenPosition(string closingAllPositions, string openingPosition)
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}{closingAllPositions}{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}{openingPosition}{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}100{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}60{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}120" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
        await instance.CheckForSignal();

        Assert.IsType<bool>(instance.ShouldOpenPosition());
    }

    public static readonly List<object[]> TestGetSLPriceData = new(){
        new object[] {"10"},
        new object[] {"10.6"},
        new object[] {"  10.6"},
        new object[] {"10.6"}
    };

    [Theory]
    [MemberData(nameof(TestGetSLPriceData))]
    public async Task TestGetSLPrice(string slPrice)
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}10.5{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}{slPrice}{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}60{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}120" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
        await instance.CheckForSignal();

        Assert.IsType<float>(instance.GetSLPrice());
    }

    public static readonly List<object[]> TestGetSLPriceFailureData = new(){
        new object[] {"-10.1"},
        new object[] {"0"},
        new object[] {"aa"}
    };

    [Theory]
    [MemberData(nameof(TestGetSLPriceFailureData))]
    public async Task TestGetSLPriceFailure(string slPrice)
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}{slPrice}{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}60{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}120" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

        await Assert.ThrowsAsync<MessageParseException>(instance.CheckForSignal);
    }

    public static readonly List<object[]> TestGetTimeFrameData = new(){
        new object[] {"60"},
        new object[] {"  60"},
        new object[] {"60  "}
    };

    [Theory]
    [MemberData(nameof(TestGetTimeFrameData))]
    public async Task TestGetTimeFrame(string timeFrame)
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}10.5{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}100{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}{timeFrame}{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}120" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
        await instance.CheckForSignal();

        Assert.IsType<int>(instance.GetTimeFrame());
    }

    public static readonly List<object[]> TestGetTimeFrameFailureData = new(){
        new object[] {"10.5"},
        new object[] {"10a"},
        new object[] {"-10.1"},
        new object[] {"0"},
        new object[] {"aa"}
    };

    [Theory]
    [MemberData(nameof(TestGetTimeFrameFailureData))]
    public async Task TestGetTimeFrameFailure(string timeFrame)
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}100{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}{timeFrame}{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}120" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

        await Assert.ThrowsAsync<MessageParseException>(instance.CheckForSignal);
    }

    public static readonly List<object[]> TestGetTPPriceData = new(){
        new object[] {"10"},
        new object[] {"10.6"},
        new object[] {"  10.6"},
        new object[] {"10.6"}
    };

    [Theory]
    [MemberData(nameof(TestGetTPPriceData))]
    public async Task TestGetTPPrice(string tpPrice)
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}10.5{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}100{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}60{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}{tpPrice}" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
        await instance.CheckForSignal();

        Assert.IsType<float>(instance.GetTPPrice());
    }

    [Fact]
    public async Task TestGetTPPriceNoTP()
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}10.5{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}100{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}60" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
        await instance.CheckForSignal();

        Assert.Null(instance.GetTPPrice());
    }

    public static readonly List<object[]> TestGetTPPriceFailureData = new(){
        new object[] {"-10.1"},
        new object[] {"0"},
        new object[] {"aa"}
    };

    [Theory]
    [MemberData(nameof(TestGetTPPriceFailureData))]
    public async Task TestGetTPPriceFailure(string tpPrice)
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}100{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}60{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}{tpPrice}" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

        await Assert.ThrowsAsync<MessageParseException>(instance.CheckForSignal);
    }

    [Fact]
    public async Task TestCheckForSignalOk()
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}100{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}60{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}120" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

        Assert.True(await instance.CheckForSignal());
    }

    [Fact]
    public async Task TestCheckForSignalFailureNoMessage()
    {
        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(null));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

        Assert.False(await instance.CheckForSignal());
    }

    [Fact]
    public async Task TestCheckForSignalFailureOldId()
    {
        Message message = new()
        {
            Id = "1",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}100{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}60{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}120" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

        Assert.True(await instance.CheckForSignal());
        Assert.False(await instance.CheckForSignal());
    }

    [Fact]
    public async Task TestCheckForSignalFailureProvider()
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}100{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}60{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}120" + MESSAGE_DELIMITER,
            From = "some other provider",
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

        await Assert.ThrowsAsync<InvalidProviderException>(instance.CheckForSignal);
    }

    [Fact]
    public async Task TestCheckForSignalFailureBadMessage()
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = "bad message",
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

        Assert.False(await instance.CheckForSignal());
    }

    [Fact]
    public async Task TestCheckForSignalFailureOpenCloseSignal()
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}100{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}60{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}120" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

        await Assert.ThrowsAsync<InvalidSignalException>(instance.CheckForSignal);

        message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}100{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}60{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}120" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-30)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        instance = Instantiate();

        await Assert.ThrowsAsync<InvalidSignalException>(instance.CheckForSignal);
    }

    [Fact]
    public async Task TestCheckForSignalFailureExpiredSignal()
    {
        Message message = new()
        {
            Id = "id",
            Subject = "subject",
            Body = MESSAGE_DELIMITER + $"_allowingParallelPositions{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_closingAllPositions{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_direction{KEY_VALUE_PAIR_DELIMITER}1{FIELD_DELIMITER}_leverage{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_margin{KEY_VALUE_PAIR_DELIMITER}10{FIELD_DELIMITER}_openingPosition{KEY_VALUE_PAIR_DELIMITER}0{FIELD_DELIMITER}_slPrice{KEY_VALUE_PAIR_DELIMITER}100{FIELD_DELIMITER}_timeFrame{KEY_VALUE_PAIR_DELIMITER}60{FIELD_DELIMITER}_tpPrice{KEY_VALUE_PAIR_DELIMITER}120" + MESSAGE_DELIMITER,
            From = provider,
            SentAt = DateTime.UtcNow.AddSeconds(-61)
        };

        Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<Message?>(message));

        bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

        await Assert.ThrowsAsync<ExpiredSignalException>(instance.CheckForSignal);
    }
}
