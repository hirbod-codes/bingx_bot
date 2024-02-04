using bot.src.Bots.UtBot.Models;
using bot.src.Brokers;
using bot.src.Data.Models;
using bot.src.MessageStores;
using bot.src.RiskManagement;
using bot.src.Util;
using Serilog;

namespace bot.src.Bots.UtBot;

public class Bot : IBot
{
    private readonly IBroker _broker;
    private readonly ILogger _logger;
    private readonly ITime _time;
    private readonly IMessageStore _messageStore;
    private readonly BotOptions _botOptions;
    private readonly IRiskManagement _riskManagement;
    private string? _firstMessageDirection = null;
    private string? _secondMessageId = null;
    private string? _secondMessageDirection = null;

    public Bot(IBotOptions botOptions, IBroker broker, ITime time, IMessageStore messageStore, IRiskManagement riskManagement, ILogger logger)
    {
        _logger = logger.ForContext<Bot>();
        _time = time;
        _broker = broker;
        _messageStore = messageStore;
        _botOptions = (botOptions as BotOptions)!;
        _riskManagement = riskManagement;
    }

    public async Task Run()
    {
        try
        {
            _logger.Information("Bot started at: {dateTime}", DateTime.UtcNow.ToString());

            await _time.StartTimer(_botOptions.TimeFrame, async (o, args) => await Tick());

            while (true)
            {
                string? userInput = System.Console.ReadLine();

                if (userInput is not null && userInput.ToLower() == "exit")
                    return;
            }
        }
        finally { _logger.Information("Bot ended at: {dateTime}", DateTime.UtcNow.ToString()); }
    }

    public async Task Tick()
    {
        _logger.Information("Ticking at: {dateTime}", DateTime.UtcNow.ToString());

        IUtBotMessage? utBotMessage = await CheckForSignal();

        if (utBotMessage is null)
        {
            _logger.Information("No signal");
            return;
        }

        if (!await _riskManagement.PermitOpenPosition())
        {
            _logger.Information("Risk management rejects opening a position, skipping...");
            return;
        }

        decimal margin = _riskManagement.GetMargin();
        decimal leverage = _riskManagement.GetLeverage((await _broker.GetCurrentCandle()).Close, utBotMessage.SlPrice);

        _logger.Information("Opening a market position...");

        await _broker.OpenMarketPosition(margin, leverage, utBotMessage.Direction, utBotMessage.SlPrice);

        _logger.Information("market position is opened.");
    }

    private async Task<IUtBotMessage?> CheckForSignal()
    {
        _logger.Information("Checking for signals.");

        IMessage? rawMessage = await _messageStore.GetLastMessage(from: _botOptions.Provider);

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

    private bool ValidateMessage(IUtBotMessage? message)
    {

        if (message is null)
        {
            _logger.Information("Message has no signal!");
            return false;
        }

        if (message.From != _botOptions.Provider)
        {
            _logger.Information("Received a message with invalid provider.");
            throw new InvalidProviderException();
        }

        if (message.Id == _secondMessageId)
        {
            _logger.Information("This message is already processed.");
            return false;
        }

        if (message.Direction != PositionDirection.SHORT || message.Direction != PositionDirection.LONG)
        {
            _logger.Information("Invalid position direction provided by the message.");
            return false;
        }

        _firstMessageDirection = _secondMessageDirection;
        _secondMessageId = message.Id;
        _secondMessageDirection = message.Direction;

        if (message.SentAt.AddSeconds(_botOptions.TimeFrame) < _time.GetUtcNow())
        {
            ExpiredSignalException ex = new();
            _logger.Error(ex, "This message is expired(too old for this time frame).");
            throw new ExpiredSignalException();
        }

        return _firstMessageDirection == _secondMessageDirection;
    }
}
