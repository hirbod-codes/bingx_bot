using bot.src.Bots.UtBot.Models;
using bot.src.Brokers;
using bot.src.Data.Models;
using bot.src.MessageStores;
using bot.src.Notifiers;
using bot.src.RiskManagement;
using bot.src.Util;
using Serilog;

namespace bot.src.Bots.UtBot;

public class Bot : IBot
{
    private readonly IBroker _broker;
    private readonly ILogger _logger;
    private readonly INotifier _notifier;
    private readonly ITime _time;
    private readonly IMessageStore _messageStore;
    private readonly BotOptions _botOptions;
    private readonly IRiskManagement _riskManagement;
    private string? _messageId = null;

    public Bot(IBotOptions botOptions, IBroker broker, ITime time, IMessageStore messageStore, IRiskManagement riskManagement, ILogger logger, INotifier notifier)
    {
        _logger = logger.ForContext<Bot>();
        _time = time;
        _broker = broker;
        _messageStore = messageStore;
        _botOptions = (botOptions as BotOptions)!;
        _riskManagement = riskManagement;
        _notifier = notifier;
    }

    public async Task Run()
    {
        try
        {
            _logger.Information("Bot started at: {dateTime}", DateTime.UtcNow.ToString());
            await _notifier.SendMessage($"Bot started at: {DateTime.UtcNow}");

            await _time.StartTimer(_botOptions.TimeFrame, async (o, args) =>
            {
                await _time.Sleep(7000);
                await Tick();
            });

            while (true)
            {
                string? userInput = System.Console.ReadLine();

                if (userInput is not null && userInput.ToLower() == "exit")
                    return;
            }
        }
        finally
        {
            _logger.Information("Bot terminated at: {dateTime}", DateTime.UtcNow.ToString());
            await _notifier.SendMessage($"Bot terminated at: {DateTime.UtcNow}");
        }
    }

    public async Task Tick()
    {
        _logger.Information("Ticking at: {dateTime}", DateTime.UtcNow.ToString());

        IUtBotMessage? utBotMessage = await CheckForSignal();

        if (utBotMessage is null)
        {
            _logger.Information("No signal.");
            return;
        }

        decimal entryPrice = await _broker.GetLastPrice();

        if (!await _riskManagement.IsPositionAcceptable(entryPrice, utBotMessage.SlPrice))
        {
            _logger.Information("Risk management rejects opening a position, skipping...");
            return;
        }

        decimal margin = _riskManagement.GetMargin();
        decimal leverage = _riskManagement.CalculateLeverage(await _broker.GetLastPrice(), utBotMessage.SlPrice);

        await CloseAllPositions();
        await OpenMarketPosition(margin, leverage, utBotMessage.Direction, utBotMessage.SlPrice);
    }

    private async Task<IUtBotMessage?> CheckForSignal()
    {
        _logger.Information("Checking for signals.");

        IMessage? rawMessage = await GetLastMessage(from: _botOptions.Provider);

        if (rawMessage is null)
        {
            _logger.Information("No message found.");
            return null;
        }

        IUtBotMessage? message = IUtBotMessage.CreateMessage(new UtBotBotMessage(), rawMessage);

        if (!ValidateMessage(message))
            return null;

        _logger.Information("Signal received.");
        return message;
    }

    private async Task<IMessage?> GetLastMessage(string from)
    {
        Exception? exception = null;

        for (int i = 0; i < _botOptions.MessageStoreFailureRetryCount; i++)
            try { return await _messageStore.GetLastMessage(from); }
            catch (MessageStoreException ex)
            {
                exception = ex;
                _logger.Error(ex, "A message store failure encountered, retrying...");
                await _time.Sleep(_botOptions.MessageStoreFailureRetryInterval);
            }

        string errorMessage = "failure in message store api has exceeded the configured retry count, terminating...";
        _logger.Error(errorMessage);

        if (!_botOptions.ShouldTerminateAfterMessageStoreFailure)
        {
            _logger.Information("Skipping...");
            return null;
        }

        _logger.Information("Terminating...");
        await _notifier.SendMessage(errorMessage);
        throw exception!;
    }

    private bool ValidateMessage(IUtBotMessage? message)
    {
        if (message is null)
        {
            _logger.Information("Message has no signal!");
            return false;
        }

        if (!message.From.Contains(_botOptions.Provider))
        {
            _logger.Information("Received a message with invalid provider. message provider:{messageProvider}, bot provider:{botProvider}", message.From, _botOptions.Provider);
            return false;
        }

        if (message.Id == _messageId)
        {
            _logger.Information("This message is already processed.");
            return false;
        }

        if (message.Direction != PositionDirection.SHORT && message.Direction != PositionDirection.LONG)
        {
            _logger.Information("Invalid position direction provided by the message.");
            return false;
        }

        if (_time.GetUtcNow().AddSeconds(-1 * _botOptions.TimeFrame) > message.SentAt)
        {
            _logger.Information("This message is expired(too old for this time frame), skipping...");
            return false;
        }

        _messageId = message.Id;

        return true;
    }

    private async Task CloseAllPositions()
    {
        _logger.Information("Closing all market positions...");
        Exception? exception = null;

        for (int i = 0; i < _botOptions.BrokerFailureRetryCount; i++)
            try
            {
                await _broker.CloseAllPositions();
                _logger.Information("All market positions has been closed...");
                return;
            }
            catch (BrokerException ex)
            {
                exception = ex;
                _logger.Error(ex, "A broker failure encountered, retrying...");
                await _time.Sleep(_botOptions.BrokerFailureRetryInterval);
            }

        string message = "failure in broker api has exceeded the configured retry count.";
        _logger.Error(message);

        if (!_botOptions.ShouldTerminateAfterBrokerFailure)
        {
            _logger.Information("Skipping...");
            return;
        }

        _logger.Information("Terminating...");
        await _notifier.SendMessage(message);
        throw exception!;
    }

    private async Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice)
    {
        _logger.Information("Opening a market position...");

        Exception? exception = null;

        for (int i = 0; i < _botOptions.BrokerFailureRetryCount; i++)
            try
            {
                await _broker.OpenMarketPosition(margin, leverage, direction, slPrice);
                _logger.Information("A market position has opened.");
                return;
            }
            catch (BrokerException ex)
            {
                exception = ex;
                _logger.Error(ex, "A broker failure encountered, retrying...");
                await _time.Sleep(_botOptions.BrokerFailureRetryInterval);
            }

        string message = "failure in broker api has exceeded the configured retry count, terminating...";
        _logger.Error(message);

        if (!_botOptions.ShouldTerminateAfterBrokerFailure)
        {
            _logger.Information("Skipping...");
            return;
        }

        _logger.Information("Terminating...");
        await _notifier.SendMessage(message);
        throw exception!;
    }
}
