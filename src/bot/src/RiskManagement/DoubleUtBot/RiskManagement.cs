using bot.src.Brokers;
using bot.src.Data.Models;
using bot.src.RiskManagement.DoubleUtBot.Exceptions;
using bot.src.Util;

namespace bot.src.RiskManagement.DoubleUtBot;

public class RiskManagement : IRiskManagement
{
    private readonly RiskManagementOptions _riskManagementOptions;
    private readonly IBroker _broker;
    private readonly ITime _time;

    public RiskManagement(IRiskManagementOptions riskManagementOptions, IBroker broker, ITime time)
    {
        _riskManagementOptions = (riskManagementOptions as RiskManagementOptions)!;
        _broker = broker;
        _time = time;
    }

    public int GetUnacceptableOrdersCount() => 0;

    private decimal GetMaximumLeverage()
    {
        if (_riskManagementOptions.BrokerCommission <= 0)
            return 200000;
        else
            return _riskManagementOptions.SLPercentages * (_riskManagementOptions.CommissionPercentage / 100.0m) / (100.0m * _riskManagementOptions.BrokerCommission);
    }

    public decimal CalculateLeverage(decimal entryPrice, decimal slPrice) => _riskManagementOptions.SLPercentages * entryPrice / 100.0m / Math.Abs(entryPrice - slPrice);

    public decimal CalculateTpPrice(decimal leverage, decimal entryPrice, string direction)
    {
        decimal delta = (_riskManagementOptions.RiskRewardRatio * entryPrice * (_riskManagementOptions.SLPercentages / 100.0m) / leverage) + (_riskManagementOptions.BrokerCommission * entryPrice);

        if (direction == PositionDirection.LONG)
            return delta + entryPrice;
        else
            return entryPrice - delta;
    }

    public decimal GetMargin() => _riskManagementOptions.Margin;

    public decimal GetMarginRelativeToLimitedLeverage(decimal entryPrice, decimal slPrice) => throw new NotImplementedException();

    public async Task<bool> IsPositionAcceptable(decimal entryPrice, decimal slPrice)
    {
        if (GetMaximumLeverage() < CalculateLeverage(entryPrice, slPrice))
            return false;

        if (_riskManagementOptions.GrossProfitLimit == 0 && _riskManagementOptions.GrossLossLimit == 0 && _riskManagementOptions.NumberOfConcurrentPositions == 0)
            return true;

        decimal grossLossPerPosition = _riskManagementOptions.Margin * (_riskManagementOptions.SLPercentages / 100m);

        if (_riskManagementOptions.GrossLossLimit < grossLossPerPosition)
            throw new InvalidRiskManagementException();

        Task<IEnumerable<Position?>> closedPositionsForLossTask = _broker.GetClosedPositions(_time.GetUtcNow().Date);
        Task<IEnumerable<Position?>> closedPositionsForProfitTask = _broker.GetClosedPositions(_time.GetUtcNow().Date);
        Task<IEnumerable<Position?>> openedPositionsTask = _broker.GetOpenPositions();
        await Task.WhenAll(closedPositionsForLossTask, closedPositionsForProfitTask, openedPositionsTask);
        IEnumerable<Position> closedPositionsForLoss = closedPositionsForLossTask.Result.Where(o => o != null)!;
        IEnumerable<Position> closedPositionsForProfit = closedPositionsForProfitTask.Result.Where(o => o != null)!;
        IEnumerable<Position> openedPositions = openedPositionsTask.Result.Where(o => o != null)!;

        if (_riskManagementOptions.NumberOfConcurrentPositions != 0 && openedPositions.Count() >= _riskManagementOptions.NumberOfConcurrentPositions)
            return false;

        if (_riskManagementOptions.GrossLossLimit == 0 && _riskManagementOptions.GrossProfitLimit == 0)
            return false;

        // Adding another grossLossPerPosition because of current possible position
        decimal openedPositionsMaximumLoss = (openedPositions.Count() * grossLossPerPosition) + grossLossPerPosition;

        if (_riskManagementOptions.GrossLossLimit != 0 && openedPositionsMaximumLoss >= _riskManagementOptions.GrossLossLimit)
            return false;

        decimal grossLoss = PnLAnalysis.PnLAnalysis.GetGrossLoss(closedPositionsForLoss) + openedPositionsMaximumLoss;

        if (_riskManagementOptions.GrossLossLimit != 0 && grossLoss >= Math.Abs(_riskManagementOptions.GrossLossLimit))
            return false;

        decimal grossProfit = Math.Abs(grossLoss - PnLAnalysis.PnLAnalysis.GetGrossProfit(closedPositionsForProfit));

        if (_riskManagementOptions.GrossProfitLimit != 0 && grossProfit >= _riskManagementOptions.GrossProfitLimit)
            return false;

        return true;
    }

    public decimal CalculateSlPrice(decimal leverage, decimal entryPrice, string direction)
    {
        throw new NotImplementedException();
    }
}
