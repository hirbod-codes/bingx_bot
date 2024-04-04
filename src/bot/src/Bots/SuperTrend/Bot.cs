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
                    await CloseAllPositions(_botOptions.RetryCount);
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
        decimal margin = _riskManagement.GetMargin();
        decimal leverage = _riskManagement.CalculateLeverage(entryPrice, 0);
        decimal slPrice = _riskManagement.CalculateTpPrice(leverage, entryPrice, message.Direction);
        decimal tpPrice = _riskManagement.CalculateSlPrice(leverage, entryPrice, message.Direction);

        if (!await _riskManagement.IsPositionAcceptable(entryPrice, slPrice))
        {
            _logger.Information("Risk management rejects opening a position, skipping...");
            return;
        }

        _logger.Information("Opening a market position...");

        await OpenMarketPosition(entryPrice, margin, leverage, message.Direction, slPrice, tpPrice, _botOptions.RetryCount);

        _logger.Information("market position is opened.");
    }

    private async Task CloseAllPositions(int retryCount)
    {
        BrokerException? brokerException = null;
        while (retryCount > 0)
        {
            try
            {
                retryCount--;
                await _broker.CloseAllPositions();
                return;
            }
            catch (BrokerException ex)
            {
                brokerException = ex;
                if (retryCount > 0)
                    _logger.Information("Operation failed, retrying...");
            }
        }

        _logger.Error(brokerException, "Operation failed, retrying...");
        throw brokerException!;
    }

    private async Task OpenMarketPosition(decimal entryPrice, decimal margin, decimal leverage, string direction, decimal slPrice, decimal? tpPrice, int retryCount)
    {
        BrokerException? brokerException = null;
        while (retryCount > 0)
        {
            try
            {
                retryCount--;

                if (tpPrice == null)
                    await _broker.OpenMarketPosition(entryPrice, margin, leverage, direction, slPrice);
                else
                    await _broker.OpenMarketPosition(entryPrice, margin, leverage, direction, slPrice, (decimal)tpPrice!);
                return;
            }
            catch (BrokerException ex)
            {
                brokerException = ex;
                if (retryCount > 0)
                    _logger.Information("Operation failed, retrying...");
            }
        }

        _logger.Error(brokerException, "Operation failed, retrying...");
        throw brokerException!;
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
