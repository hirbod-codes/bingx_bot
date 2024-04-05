using bot.src.MessageStores;
using Moq;
using ILogger = Serilog.ILogger;

namespace unit_tests;

[CollectionDefinition("Strategy")]
public class Strategy : ICollectionFixture<Fixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

public class Fixture
{
    public Mock<ILogger> ILogger { get; private set; } = new();
    public Mock<IMessageStore> IMessageStore { get; private set; } = new();

    public void Reset()
    {
        IMessageStore = new Mock<IMessageStore>();
        ILogger = new Mock<ILogger>();
    }
}
