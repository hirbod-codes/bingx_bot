using bot.src.Brokers;
using bot.src.Data.Models;
using bot.src.RiskManagement.SmmaRsi.Exceptions;
using bot.src.Util;

namespace bot.src.RiskManagement.SmmaRsi;

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

    public decimal CalculateLeverage(decimal entryPrice, decimal slPrice) => _riskManagementOptions.SLPercentages * entryPrice / (100.0m * Math.Abs(entryPrice - slPrice));

    public decimal CalculateTpPrice(decimal leverage, decimal entryPrice, string direction)
    {
        throw new NotImplementedException();
    }

    public decimal GetMargin() => _riskManagementOptions.Margin;

    public decimal GetMarginRelativeToLimitedLeverage(decimal entryPrice, decimal slPrice)
    {
        throw new NotImplementedException();
    }

    private decimal GetMaximumLeverage() => _riskManagementOptions.SLPercentages;

    public async Task<bool> PermitOpenPosition(decimal entryPrice, decimal slPrice)
    {
        if (GetMaximumLeverage() > CalculateLeverage(entryPrice, slPrice))
            return false;

        if (_riskManagementOptions.GrossProfitLimit == 0 && _riskManagementOptions.GrossLossLimit == 0 && _riskManagementOptions.NumberOfConcurrentPositions == 0)
            return true;

        decimal grossLossPerPosition = _riskManagementOptions.Margin * (_riskManagementOptions.SLPercentages / 100m);

        if (_riskManagementOptions.GrossLossLimit != 0 && _riskManagementOptions.GrossLossLimit < grossLossPerPosition)
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

        if (_riskManagementOptions.GrossProfitLimit != 0)
        {
            decimal grossProfit = PnLAnalysis.PnLAnalysis.GetGrossProfit(closedPositionsForProfit);

            if (grossProfit >= _riskManagementOptions.GrossProfitLimit)
                return false;
        }

        if (_riskManagementOptions.GrossLossLimit != 0)
        {
            // Adding another grossLossPerPosition because of current possible position
            decimal openedPositionsMaximumLoss = (openedPositions.Count() * grossLossPerPosition) + grossLossPerPosition;

            if (openedPositionsMaximumLoss >= _riskManagementOptions.GrossLossLimit)
                return false;

            decimal grossLoss = PnLAnalysis.PnLAnalysis.GetGrossLoss(closedPositionsForLoss) + openedPositionsMaximumLoss;

            if (grossLoss >= Math.Abs(_riskManagementOptions.GrossLossLimit))
                return false;
        }

        return true;
    }
}
