using email_api.Models;
using email_api.src;
using Moq;
using Serilog;
using strategies.src;

namespace TestSignals;

public class TestGeneralSignal
{
    private static readonly string FormattedDateTime = "2020-1-1 10:20:00";
    private static readonly string Side = "long";
    private static readonly float Leverage = 154.26741f;
    private static readonly float Margin = 154.26741f;
    private static readonly float TPPrice = 154.26741f;
    private static readonly float SLPrice = 154.26741f;

    public static IEnumerable<object?[]> MessageBody =>
        new List<object?[]>
        {
            new object?[] {
                new Email()
                {
                    MailDateTime = DateTime.Parse("2020-1-1T10:20:00"),
                    Body = GeneralSignals.EMAIL_SEPARATOR +
                    $"side={Side}" + GeneralSignals.MESSAGE_DELIMITER +
                    $"leverage={Leverage}" + GeneralSignals.MESSAGE_DELIMITER +
                    $"margin={Margin}" + GeneralSignals.MESSAGE_DELIMITER +
                    $"sl={SLPrice}" + GeneralSignals.MESSAGE_DELIMITER +
                    $"tp={TPPrice}" +
                    GeneralSignals.EMAIL_SEPARATOR,
                }
            },
            new object?[] {
                new Email()
                {
                    MailDateTime = DateTime.Parse("2020-1-1T10:20:00"),
                    Body = GeneralSignals.EMAIL_SEPARATOR +
                    $"side={Side}" + GeneralSignals.MESSAGE_DELIMITER +
                    $"leverage={Leverage}" + GeneralSignals.MESSAGE_DELIMITER +
                    $"margin={Margin}" + GeneralSignals.MESSAGE_DELIMITER +
                    $"sl={SLPrice}" +
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
        Assert.Equal((int)Leverage, actualLeverage);
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
        Assert.Equal(Margin, actualMargin);
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
        Assert.Equal(SLPrice, actualSLPrice);
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
            Assert.Equal(TPPrice, actualTPPrice);
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

        bool actualSignal = generalSignals.IsSignalLong();
        Assert.Equal(Side.ToLower() == "long", actualSignal);
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
        Assert.Equal(email.MailDateTime, actualSignalTime);
    }
}
