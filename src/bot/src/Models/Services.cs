using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Data;
using bot.src.MessageStores;
using bot.src.Notifiers;
using bot.src.RiskManagement;
using bot.src.Runners;
using bot.src.Strategies;
using bot.src.Util;

namespace bot.src.Models;

public class Services
{
    public IPositionRepository? PositionRepository { get; set; }
    public IMessageRepository? MessageRepository { get; set; }
    public INotifier? Notifier { get; set; }
    public ITime? Time { get; set; }
    public IBroker? Broker { get; set; }
    public IMessageStore? MessageStore { get; set; }
    public IRiskManagement? RiskManagement { get; set; }
    public IStrategy? Strategy { get; set; }
    public IBot? Bot { get; set; }
    public IRunner? Runner { get; set; }
}
