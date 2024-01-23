using bot.src.Bots.General.Models;
using bot.src.Brokers;
using bot.src.Data.Models;
using bot.src.MessageStores;
using bot.src.RiskManagement;
using bot.src.Util;
using Serilog;

namespace bot.src.Bots.General;

public class Bot : IBot
{
    private readonly IBroker _broker;
    private readonly ILogger _logger;
    private readonly ITime _time;
    private readonly IMessageStore _messageStore;
    private readonly BotOptions _generalBotOptions;
    private readonly IRiskManagement _riskManagement;
    private string? _previousMessageId = null;

    public Bot(IBotOptions generalBotOptions, IBroker broker, ITime time, IMessageStore messageStore, IRiskManagement riskManagement, ILogger logger)
    {
        _logger = logger.ForContext<Bot>();
        _time = time;
        _broker = broker;
        _messageStore = messageStore;
        _generalBotOptions = (generalBotOptions as BotOptions)!;
        _riskManagement = riskManagement;
    }

    public async Task Run()
    {
        try
        {
            _logger.Information("Bot started at: {dateTime}", DateTime.UtcNow.ToString());

            await _time.StartTimer(_generalBotOptions.TimeFrame, async (o, args) => await Tick());

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

        IGeneralMessage? generalMessage = await CheckForSignal();

        if (generalMessage is null)
        {
            _logger.Information("No signal");
            return;
        }

        if ((!generalMessage.OpeningPosition && !generalMessage.ClosingAllPositions) || (generalMessage.OpeningPosition && generalMessage.ClosingAllPositions))
        {
            _logger.Information("Invalid signal, ShouldOpenPosition and ShouldCloseAllPositions variables are both true at the same time!");
            return;
        }

        if (generalMessage.ClosingAllPositions)
        {
            _logger.Information("Closing all of the open positions...");
            await _broker.CloseAllPositions();
            _logger.Information("open positions are closed.");
            return;
        }

        if (!generalMessage.AllowingParallelPositions && (await _broker.GetOpenPositions()).Any())
        {
            _logger.Information("Parallel positions is not allowed, skip until the position is closed.");
            return;
        }

        IEnumerable<Position> positions = await _broker.GetOpenPositions();
        if (
            (generalMessage.Direction == PositionDirection.LONG && positions.Any(o => o.PositionDirection == PositionDirection.SHORT)) ||
            (generalMessage.Direction == PositionDirection.SHORT && positions.Any(o => o.PositionDirection == PositionDirection.LONG))
        )
        {
            _logger.Information("There are open positions with opposite direction from the provided signal, skipping...");
            return;
        }

        if (!await _riskManagement.PermitOpenPosition())
        {
            _logger.Information("Risk management rejects opening a position, skipping...");
            return;
        }

        decimal margin = _riskManagement.GetMargin();
        decimal leverage = _riskManagement.GetLeverage((await _broker.GetCurrentCandle()).Close, generalMessage.SlPrice);

        _logger.Information("Opening a market position...");

        if (generalMessage.TpPrice is null)
            await _broker.OpenMarketPosition(margin, leverage, generalMessage.Direction, generalMessage.SlPrice);
        else
            await _broker.OpenMarketPosition(margin, leverage, generalMessage.Direction, generalMessage.SlPrice, (decimal)generalMessage.TpPrice!);

        _logger.Information("market position is opened.");
    }

    private async Task<IGeneralMessage?> CheckForSignal()
    {
        _logger.Information("Checking for signals.");

        IMessage? rawMessage = await _messageStore.GetLastMessage(from: _generalBotOptions.Provider);

        if (rawMessage is null)
        {
            _logger.Information("No message found.");
            return null;
        }

        IGeneralMessage? message = IGeneralMessage.CreateMessage(new GeneralBotMessage(), rawMessage);

        if (!ValidateMessage(message))
            return null;

        _logger.Information("Signal received.");
        return message;
    }

    private bool ValidateMessage(IGeneralMessage? message)
    {

        if (message is null)
        {
            _logger.Information("Message has no signal!");
            return false;
        }

        if (message.From != _generalBotOptions.Provider)
        {
            _logger.Information("Received a message with invalid provider.");
            throw new InvalidProviderException();
        }

        if (message.Id == _previousMessageId)
        {
            _logger.Information("This message is already processed.");
            return false;
        }
        else
            _previousMessageId = message.Id;

        if ((message.OpeningPosition && message.ClosingAllPositions) || (!message.OpeningPosition && !message.ClosingAllPositions))
        {
            InvalidSignalException ex = new();
            _logger.Error(ex, "This message is already processed.");
            throw ex;
        }

        if (message.SentAt.AddSeconds(_generalBotOptions.TimeFrame) < _time.GetUtcNow())
        {
            ExpiredSignalException ex = new();
            _logger.Error(ex, "This message is expired(too old for this time frame).");
            throw new ExpiredSignalException();
        }

        return true;
    }
}
