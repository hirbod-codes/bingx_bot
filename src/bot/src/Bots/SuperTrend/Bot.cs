using bot.src.Bots.SuperTrendV1.Models;
using bot.src.Brokers;
using bot.src.Data.Models;
using bot.src.MessageStores;
using bot.src.Notifiers;
using bot.src.RiskManagement;
using bot.src.Util;
using Serilog;

namespace bot.src.Bots.SuperTrendV1;

public class Bot : IBot
{
    private readonly IBroker _broker;
    private readonly ILogger _logger;
    private readonly ITime _time;
    private readonly IMessageStore _messageStore;
    private readonly BotOptions _botOptions;
    private readonly IRiskManagement _riskManagement;
    private readonly INotifier _notifier;
    private string? _previousMessageId = null;

    public Bot(IBotOptions generalBotOptions, IBroker broker, ITime time, IMessageStore messageStore, IRiskManagement riskManagement, ILogger logger, INotifier notifier)
    {
        _logger = logger.ForContext<Bot>();
        _time = time;
        _broker = broker;
        _messageStore = messageStore;
        _botOptions = (generalBotOptions as BotOptions)!;
        _riskManagement = riskManagement;
        _notifier = notifier;
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

        Message? message = await CheckForSignal();

        if (message is null)
        {
            _logger.Information("No signal");
            return;
        }

        if ((!message.OpeningPosition && !message.ClosingAllPositions) || (message.OpeningPosition && message.ClosingAllPositions))
        {
            _logger.Information("Invalid signal, ShouldOpenPosition and ShouldCloseAllPositions variables are both true at the same time!");
            return;
        }

        if (message.ClosingAllPositions)
        {
            _logger.Information("Closing all of the open positions...");
            await _broker.CloseAllPositions();
            _logger.Information("open positions are closed.");
            return;
        }

        IEnumerable<Position>? openPositions = null;

        if (!message.AllowingParallelPositions)
        {
            openPositions = (await _broker.GetOpenPositions()).Where(o => o != null)!;
            if (openPositions.Any())
            {
                if (_botOptions.ShouldSkipOnParallelPositionRequest)
                {
                    _logger.Information("Parallel positions is not allowed, Skipping...");
                    return;
                }
                else
                {
                    _logger.Information("Parallel positions is not allowed, Closing all of the open positions...");
                    await DoOperationWithRetry(new Task(() => _broker.CloseAllPositions()), _botOptions.RetryCount);
                    _logger.Information("open positions are closed.");
                    openPositions = Array.Empty<Position>();
                }
            }
        }

        openPositions ??= (await _broker.GetOpenPositions()).Where(o => o != null)!;

        if (
            (message.Direction == PositionDirection.LONG && openPositions.Any(o => o.PositionDirection == PositionDirection.SHORT)) ||
            (message.Direction == PositionDirection.SHORT && openPositions.Any(o => o.PositionDirection == PositionDirection.LONG))
        )
        {
            _logger.Information("There are open positions with opposite direction from the provided signal, skipping...");
            return;
        }

        decimal entryPrice = message.EntryPrice;

        if (!await _riskManagement.IsPositionAcceptable(entryPrice, message.SlPrice))
        {
            _logger.Information("Risk management rejects opening a position, skipping...");
            return;
        }

        decimal leverage = _riskManagement.CalculateLeverage(entryPrice, message.SlPrice);
        decimal margin = _riskManagement.GetMargin();

        _logger.Information("Opening a market position...");

        if (message.TpPrice == null)
            await DoOperationWithRetry(new Task(() => _broker.OpenMarketPosition(entryPrice, margin, leverage, message.Direction, message.SlPrice)), _botOptions.RetryCount);
        else
            await DoOperationWithRetry(new Task(() => _broker.OpenMarketPosition(entryPrice, margin, leverage, message.Direction, message.SlPrice, (decimal)message.TpPrice!)), _botOptions.RetryCount);

        _logger.Information("market position is opened.");
    }

    private async Task DoOperationWithRetry(Task operation, int retryCount)
    {
        try
        {
            retryCount--;
            await operation;
        }
        catch (BrokerException ex)
        {
            if (retryCount > 0)
                _logger.Information("Operation failed, retrying...");
            else
                _logger.Error(ex, "Operation failed, terminating...");
        }
    }

    private async Task<Message?> CheckForSignal()
    {
        _logger.Information("Checking for signals.");

        IEnumerable<IMessage> rawMessages = await _messageStore.GetMessages(from: _botOptions.Provider);

        if (!rawMessages.Any())
        {
            _logger.Information("No message found.");
            return null;
        }

        IMessage rawMessage = rawMessages.Last();

        if (rawMessage is null)
        {
            _logger.Information("No message found.");
            return null;
        }

        if (rawMessages.Count() >= 1000)
            await _messageStore.DeleteMessages(from: _botOptions.Provider);

        Message? message = Message.CreateMessage(new Message(), rawMessage);

        if (!ValidateMessage(message))
            return null;

        _logger.Information("Signal received.");
        return message;
    }

    private bool ValidateMessage(Message? message)
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

        if (message.Id == _previousMessageId)
        {
            _logger.Information("This message is already processed.");
            return false;
        }
        else
            _previousMessageId = message.Id;

        if ((message.OpeningPosition && message.ClosingAllPositions) || (!message.OpeningPosition && !message.ClosingAllPositions))
        {
            _logger.Information("This message is already processed.");
            return false;
        }

        if (message.SentAt.AddSeconds(_botOptions.TimeFrame) < _time.GetUtcNow())
        {
            _logger.Information("This message is expired(too old for this time frame).");
            return false;
        }

        return true;
    }
}
