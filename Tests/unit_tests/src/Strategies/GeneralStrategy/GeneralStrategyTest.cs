// using bot.src.Data.Models;
// using bot.src.MessageStores;
// using bot.src.MessageStores.InMemory.Models;
// using bot.src.Strategies.GeneralStrategy;
// using Serilog;

// namespace unit_tests.src.Strategies.GeneralStrategy;

// [Collection("Strategy")]
// public class GeneralStrategyTest
// {
//     private readonly string provider = "provider";
//     public Fixture Fixture { get; private set; }

//     public GeneralStrategyTest(Fixture fixture) => Fixture = fixture;

//     public bot.src.Strategies.GeneralStrategy.GeneralStrategy Instantiate() => new(provider, Fixture.IMessageStore.Object, new LoggerConfiguration().CreateLogger());

//     public static readonly IGeneralMessage DefaultMessage = IGeneralMessage.CreateMessage(new GeneralMessage(), new Message()
//     {
//         Id = "id",
//         From = "provider",
//         Body = IGeneralMessage.CreateMessageBody(
//             openingPosition: true,
//             allowingParallelPositions: true,
//             closingAllPositions: false,
//             PositionDirection.LONG,
//             leverage: 10,
//             margin: 10,
//             timeFrame: 60,
//             slPrice: 100,
//             tpPrice: 120
//         ),
//         SentAt = DateTime.UtcNow.AddSeconds(-30)
//     })!;

//     public static readonly List<object[]> TestIsParallelPositionsAllowedData = new(){
//         new object[] {true},
//         new object[] {false}
//     };

//     [Theory]
//     [MemberData(nameof(TestIsParallelPositionsAllowedData))]
//     public async Task TestIsParallelPositionsAllowed(bool allowingParallelPositions)
//     {
//         DefaultMessage.AllowingParallelPositions = allowingParallelPositions;

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
//         await instance.CheckForSignal();

//         Assert.IsType<bool>(instance.IsParallelPositionsAllowed());
//     }

//     public static readonly List<object[]> TestShouldCloseAllPositionsData = new(){
//         new object[] {false, true},
//         new object[] {true, false}
//     };

//     [Theory]
//     [MemberData(nameof(TestShouldCloseAllPositionsData))]
//     public async Task TestShouldCloseAllPositions(bool closingAllPositions, bool openingPosition)
//     {
//         DefaultMessage.OpeningPosition = openingPosition;
//         DefaultMessage.ClosingAllPositions = closingAllPositions;

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
//         await instance.CheckForSignal();

//         Assert.IsType<bool>(instance.ShouldCloseAllPositions());
//     }

//     public static readonly List<object[]> TestGetDirectionData = new(){
//         new object[] {true},
//         new object[] {false},
//     };

//     [Theory]
//     [MemberData(nameof(TestGetDirectionData))]
//     public async Task TestGetDirection(bool direction)
//     {
//         DefaultMessage.Direction = direction;

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
//         await instance.CheckForSignal();

//         Assert.IsType<bool>(instance.GetDirection());
//     }

//     public static readonly List<object[]> TestGetLeverageData = new(){
//         new object[] {10m},
//         new object[] {10.6m},
//     };

//     [Theory]
//     [MemberData(nameof(TestGetLeverageData))]
//     public async Task TestGetLeverage(decimal leverage)
//     {
//         DefaultMessage.Leverage = leverage;

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
//         await instance.CheckForSignal();

//         Assert.IsType<float>(instance.GetLeverage());
//     }

//     public static readonly List<object[]> TestGetLeverageFailureData = new(){
//         new object[] {-10.1m},
//         new object[] {0m},
//     };

//     [Theory]
//     [MemberData(nameof(TestGetLeverageFailureData))]
//     public async Task TestGetLeverageFailure(decimal leverage)
//     {
//         DefaultMessage.Leverage = leverage;

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

//         await Assert.ThrowsAsync<MessageParseException>(instance.CheckForSignal);
//     }

//     public static readonly List<object[]> TestGetMarginData = new(){
//         new object[] {10m},
//         new object[] {10.6m}
//     };

//     [Theory]
//     [MemberData(nameof(TestGetMarginData))]
//     public async Task TestGetMargin(decimal margin)
//     {
//         DefaultMessage.Margin = margin;

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
//         await instance.CheckForSignal();

//         Assert.IsType<float>(instance.GetMargin());
//     }

//     public static readonly List<object[]> TestGetMarginFailureData = new(){
//         new object[] {-10.1m},
//         new object[] {0m}
//     };

//     [Theory]
//     [MemberData(nameof(TestGetMarginFailureData))]
//     public async Task TestGetMarginFailure(decimal margin)
//     {
//         DefaultMessage.Margin = margin;

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

//         await Assert.ThrowsAsync<MessageParseException>(instance.CheckForSignal);
//     }

//     public static readonly List<object[]> TestShouldOpenPositionData = new(){
//         new object[] {false, true},
//         new object[] {true, false}
//     };

//     [Theory]
//     [MemberData(nameof(TestShouldOpenPositionData))]
//     public async Task TestShouldOpenPosition(bool closingAllPositions, bool openingPosition)
//     {
//         DefaultMessage.OpeningPosition = openingPosition;
//         DefaultMessage.ClosingAllPositions = closingAllPositions;

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
//         await instance.CheckForSignal();

//         Assert.IsType<bool>(instance.ShouldOpenPosition());
//     }

//     public static readonly List<object[]> TestGetSLPriceData = new(){
//         new object[] {10m},
//         new object[] {10.6m}
//     };

//     [Theory]
//     [MemberData(nameof(TestGetSLPriceData))]
//     public async Task TestGetSLPrice(decimal slPrice)
//     {
//         DefaultMessage.SlPrice = slPrice;

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
//         await instance.CheckForSignal();

//         Assert.IsType<float>(instance.GetSLPrice());
//     }

//     public static readonly List<object[]> TestGetSLPriceFailureData = new(){
//         new object[] {-10.1m},
//         new object[] {0m}
//     };

//     [Theory]
//     [MemberData(nameof(TestGetSLPriceFailureData))]
//     public async Task TestGetSLPriceFailure(decimal slPrice)
//     {
//         DefaultMessage.SlPrice = slPrice;

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

//         await Assert.ThrowsAsync<MessageParseException>(instance.CheckForSignal);
//     }

//     public static readonly List<object[]> TestGetTimeFrameData = new(){
//         new object[] {60},
//     };

//     [Theory]
//     [MemberData(nameof(TestGetTimeFrameData))]
//     public async Task TestGetTimeFrame(int timeFrame)
//     {
//         DefaultMessage.TimeFrame = timeFrame;

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
//         await instance.CheckForSignal();

//         Assert.IsType<int>(instance.GetTimeFrame());
//     }

//     public static readonly List<object[]> TestGetTimeFrameFailureData = new(){
//         new object[] {-10.1},
//         new object[] {0}
//     };

//     [Theory]
//     [MemberData(nameof(TestGetTimeFrameFailureData))]
//     public async Task TestGetTimeFrameFailure(int timeFrame)
//     {
//         DefaultMessage.TimeFrame = timeFrame;

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

//         await Assert.ThrowsAsync<MessageParseException>(instance.CheckForSignal);
//     }

//     public static readonly List<object[]> TestGetTPPriceData = new(){
//         new object[] {10m},
//         new object[] {10.6m},
//     };

//     [Theory]
//     [MemberData(nameof(TestGetTPPriceData))]
//     public async Task TestGetTPPrice(decimal tpPrice)
//     {
//         DefaultMessage.TpPrice = tpPrice;

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
//         await instance.CheckForSignal();

//         Assert.IsType<float>(instance.GetTPPrice());
//     }

//     [Fact]
//     public async Task TestGetTPPriceNoTP()
//     {
//         DefaultMessage.TpPrice = null;

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();
//         await instance.CheckForSignal();

//         Assert.Null(instance.GetTPPrice());
//     }

//     public static readonly List<object[]> TestGetTPPriceFailureData = new(){
//         new object[] {-10.1m},
//         new object[] {0m}
//     };

//     [Theory]
//     [MemberData(nameof(TestGetTPPriceFailureData))]
//     public async Task TestGetTPPriceFailure(decimal tpPrice)
//     {
//         DefaultMessage.TpPrice = tpPrice;

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

//         await Assert.ThrowsAsync<MessageParseException>(instance.CheckForSignal);
//     }

//     [Fact]
//     public async Task TestCheckForSignalOk()
//     {
//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

//         Assert.True(await instance.CheckForSignal());
//     }

//     [Fact]
//     public async Task TestCheckForSignalFailureNoMessage()
//     {
//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(null));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

//         Assert.False(await instance.CheckForSignal());
//     }

//     [Fact]
//     public async Task TestCheckForSignalFailureOldId()
//     {
//         DefaultMessage.Id = "1";
//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

//         Assert.True(await instance.CheckForSignal());
//         Assert.False(await instance.CheckForSignal());
//     }

//     [Fact]
//     public async Task TestCheckForSignalFailureProvider()
//     {
//         DefaultMessage.From = "another provider";

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

//         await Assert.ThrowsAsync<InvalidProviderException>(instance.CheckForSignal);
//     }

//     [Fact]
//     public async Task TestCheckForSignalFailureBadMessage()
//     {
//         DefaultMessage.Body = "bad message";

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

//         Assert.False(await instance.CheckForSignal());
//     }

//     [Fact]
//     public async Task TestCheckForSignalFailureOpenCloseSignal()
//     {
//         DefaultMessage.OpeningPosition = false;
//         DefaultMessage.ClosingAllPositions = false;

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

//         await Assert.ThrowsAsync<InvalidSignalException>(instance.CheckForSignal);

//         DefaultMessage.OpeningPosition = true;
//         DefaultMessage.ClosingAllPositions = true;

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         instance = Instantiate();

//         await Assert.ThrowsAsync<InvalidSignalException>(instance.CheckForSignal);
//     }

//     [Fact]
//     public async Task TestCheckForSignalFailureExpiredSignal()
//     {
//         DefaultMessage.SentAt = DateTime.UtcNow.AddSeconds(-61);

//         Fixture.IMessageStore.Setup(o => o.GetLastMessage(provider)).Returns(Task.FromResult<IMessage?>(DefaultMessage));

//         bot.src.Strategies.GeneralStrategy.GeneralStrategy instance = Instantiate();

//         await Assert.ThrowsAsync<ExpiredSignalException>(instance.CheckForSignal);
//     }
// }
