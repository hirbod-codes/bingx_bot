using Abstractions.src.Bots;
using Abstractions.src.Brokers;
using Abstractions.src.Data;
using Abstractions.src.MessageStores;
using Abstractions.src.Notifiers;
using Abstractions.src.RiskManagement;
using Abstractions.src.Runners;
using Abstractions.src.Strategies;
using Abstractions.src.Utilities;

namespace Abstractions.src.Models;

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
