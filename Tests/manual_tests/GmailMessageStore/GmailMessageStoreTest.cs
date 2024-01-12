using bot.src.MessageStores.Gmail;
using bot.src.MessageStores.Gmail.Models;
using Serilog;

namespace manual_tests;

public class GmailMessageStoreTest
{
    private readonly string _dir = "/home/hirbod/projects/bingx_ut_bot/Tests/manual_tests";

    // public static List<object[]> TestRM2Data = new(){
    //     Array.Empty<object>()
    // };

    // [Theory]
    // [MemberData(nameof(TestRM2Data))]
    [Fact]
    public async Task Test()
    {
        ILogger logger = new LoggerConfiguration().CreateLogger();

        MessageProviderOptions options = new()
        {
            ClientId = "511671693440-o2fjs4lc2pfmttanjclbq7dccdeumemv.apps.googleusercontent.com",
            ClientSecret = "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",
            DataStoreFolderAddress = "./taghallobyyyyyyyyyyyyyyy",
            OwnerGmail = "taghalloby@gmail.com",
            SignalProviderEmail = "noreply@tradingview.com"
        };

        GmailMessageStore g = new(options, logger);

        bot.src.MessageStores.Models.Message? message = await g.GetLastMessage(options.SignalProviderEmail);

        return;
    }
}
