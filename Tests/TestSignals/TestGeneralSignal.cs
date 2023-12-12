using email_api.Models;
using email_api.src;
using Moq;
using Serilog;
using strategies.src;

namespace TestSignals;

public class TestGeneralSignal
{
    public static IEnumerable<object?[]> MessageBody =>
        new List<object?[]>
        {
            new object?[] {
                new Email()
                {
                    Body = GeneralSignals.EMAIL_SEPARATOR +
                    "time=2020-1-1T10:20:00" + GeneralSignals.MESSAGE_DELIMITER +
                    "side=long" + GeneralSignals.MESSAGE_DELIMITER +
                    "leverage=50" + GeneralSignals.MESSAGE_DELIMITER +
                    "margin=200" + GeneralSignals.MESSAGE_DELIMITER +
                    $"sl={40000 - 200}" + GeneralSignals.MESSAGE_DELIMITER +
                    $"tp={40000 + 200}" +
                    GeneralSignals.EMAIL_SEPARATOR,
                }
            },
            new object?[] {
                new Email()
                {
                    Body = GeneralSignals.EMAIL_SEPARATOR +
                    "time=2020-1-1T10:20:00" + GeneralSignals.MESSAGE_DELIMITER +
                    "side=long" + GeneralSignals.MESSAGE_DELIMITER +
                    "leverage=50" + GeneralSignals.MESSAGE_DELIMITER +
                    "margin=200" + GeneralSignals.MESSAGE_DELIMITER +
                    $"sl={40000 - 200}" +
                    GeneralSignals.EMAIL_SEPARATOR,
                }
            },
        };

    [Theory]
    [MemberData(nameof(MessageBody))]
    public async Task CheckSignals(Email email)
    {
        Mock<IEmailProvider> emailProvider = new();
        Mock<ILogger> logger = new();
        string signalProviderEmail = "email";
        bool? result = null;

        GeneralSignals generalSignals = new(emailProvider.Object, logger.Object);

        emailProvider.Setup(o => o.GetSignalProviderEmail()).Returns(signalProviderEmail);
        emailProvider.Setup(o => o.GetLastEmail(signalProviderEmail)).Returns(Task.FromResult<Email?>(null));
        result = await generalSignals.CheckSignals();
        Assert.False(result);

        emailProvider.Setup(o => o.GetSignalProviderEmail()).Returns(signalProviderEmail);
        emailProvider.Setup(o => o.GetLastEmail(signalProviderEmail)).Returns(value: Task.FromResult<Email?>(email));
        result = await generalSignals.CheckSignals();
        Assert.True(result);
    }

    [Theory]
    [MemberData(nameof(MessageBody))]
    public async Task GetLeverage(Email email)
    {
        Mock<IEmailProvider> emailProvider = new();
        Mock<ILogger> logger = new();
        string signalProviderEmail = "email";

        GeneralSignals generalSignals = new(emailProvider.Object, logger.Object);

        emailProvider.Setup(o => o.GetSignalProviderEmail()).Returns(signalProviderEmail);
        emailProvider.Setup(o => o.GetLastEmail(signalProviderEmail)).Returns(value: Task.FromResult<Email?>(email));
        await generalSignals.CheckSignals();

        int actualLeverage = generalSignals.GetLeverage();
        Assert.Equal(50, actualLeverage);
    }

    [Theory]
    [MemberData(nameof(MessageBody))]
    public async Task GetMargin(Email email)
    {
        Mock<IEmailProvider> emailProvider = new();
        Mock<ILogger> logger = new();
        string signalProviderEmail = "email";

        GeneralSignals generalSignals = new(emailProvider.Object, logger.Object);

        emailProvider.Setup(o => o.GetSignalProviderEmail()).Returns(signalProviderEmail);
        emailProvider.Setup(o => o.GetLastEmail(signalProviderEmail)).Returns(value: Task.FromResult<Email?>(email));
        await generalSignals.CheckSignals();

        float actualMargin = generalSignals.GetMargin();
        Assert.Equal(200, actualMargin);
    }

    [Theory]
    [MemberData(nameof(MessageBody))]
    public async Task GetSLPrice(Email email)
    {
        Mock<IEmailProvider> emailProvider = new();
        Mock<ILogger> logger = new();
        string signalProviderEmail = "email";

        GeneralSignals generalSignals = new(emailProvider.Object, logger.Object);

        emailProvider.Setup(o => o.GetSignalProviderEmail()).Returns(signalProviderEmail);
        emailProvider.Setup(o => o.GetLastEmail(signalProviderEmail)).Returns(value: Task.FromResult<Email?>(email));
        await generalSignals.CheckSignals();

        float actualSLPrice = generalSignals.GetSLPrice();
        Assert.Equal(40000 - 200, actualSLPrice);
    }

    [Theory]
    [MemberData(nameof(MessageBody))]
    public async Task GetTPPrice(Email email)
    {
        Mock<IEmailProvider> emailProvider = new();
        Mock<ILogger> logger = new();
        string signalProviderEmail = "email";

        GeneralSignals generalSignals = new(emailProvider.Object, logger.Object);

        emailProvider.Setup(o => o.GetSignalProviderEmail()).Returns(signalProviderEmail);
        emailProvider.Setup(o => o.GetLastEmail(signalProviderEmail)).Returns(value: Task.FromResult<Email?>(email));
        await generalSignals.CheckSignals();

        float? actualTPPrice = generalSignals.GetTPPrice();
        if (actualTPPrice is not null)
            Assert.Equal(40000 + 200, actualTPPrice);
    }

    [Fact]
    public async Task Initiate()
    {
        Mock<IEmailProvider> emailProvider = new();
        Mock<ILogger> logger = new();

        GeneralSignals generalSignals = new(emailProvider.Object, logger.Object);

        emailProvider.Setup(o => o.DeleteEmails(null));
        await generalSignals.Initiate();
    }

    [Theory]
    [MemberData(nameof(MessageBody))]
    public async Task IsSignalLong(Email email)
    {
        Mock<IEmailProvider> emailProvider = new();
        Mock<ILogger> logger = new();
        string signalProviderEmail = "email";

        GeneralSignals generalSignals = new(emailProvider.Object, logger.Object);

        emailProvider.Setup(o => o.GetSignalProviderEmail()).Returns(signalProviderEmail);
        emailProvider.Setup(o => o.GetLastEmail(signalProviderEmail)).Returns(value: Task.FromResult<Email?>(email));
        await generalSignals.CheckSignals();

        DateTime actualSignalTime = generalSignals.GetSignalTime();
        Assert.Equal(DateTime.Parse("2020-1-1 10:20:00"), actualSignalTime);
    }

    [Theory]
    [MemberData(nameof(MessageBody))]
    public async Task ResetSignals(Email email)
    {
        Mock<IEmailProvider> emailProvider = new();
        Mock<ILogger> logger = new();
        string signalProviderEmail = "email";

        GeneralSignals generalSignals = new(emailProvider.Object, logger.Object);

        emailProvider.Setup(o => o.GetSignalProviderEmail()).Returns(signalProviderEmail);
        emailProvider.Setup(o => o.GetLastEmail(signalProviderEmail)).Returns(value: Task.FromResult<Email?>(email));
        await generalSignals.CheckSignals();

        generalSignals.ResetSignals();
        Assert.True(DateTime.UtcNow.AddDays(-9) > generalSignals.GetSignalTime());
        Assert.False(generalSignals.IsSignalLong());
        Assert.Equal(0, generalSignals.GetLeverage());
    }

    [Theory]
    [MemberData(nameof(MessageBody))]
    public async Task GetSignalTime(Email email)
    {
        Mock<IEmailProvider> emailProvider = new();
        Mock<ILogger> logger = new();
        string signalProviderEmail = "email";

        GeneralSignals generalSignals = new(emailProvider.Object, logger.Object);

        emailProvider.Setup(o => o.GetSignalProviderEmail()).Returns(signalProviderEmail);
        emailProvider.Setup(o => o.GetLastEmail(signalProviderEmail)).Returns(value: Task.FromResult<Email?>(email));
        await generalSignals.CheckSignals();

        DateTime actualSignalTime = generalSignals.GetSignalTime();
        Assert.Equal(DateTime.Parse("2020-1-1 10:20:00"), actualSignalTime);
    }
}
