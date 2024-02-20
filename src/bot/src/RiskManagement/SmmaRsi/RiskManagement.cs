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

    public decimal GetLeverage(decimal entryPrice, decimal slPrice) => _riskManagementOptions.SLPercentages * entryPrice / (100.0m * Math.Abs(entryPrice - slPrice));

    public decimal GetMargin() => _riskManagementOptions.Margin;

    public async Task<bool> PermitOpenPosition()
    {
        if (_riskManagementOptions.GrossProfitInterval == 0 && _riskManagementOptions.GrossLossInterval == 0 && _riskManagementOptions.NumberOfConcurrentPositions == 0)
            return true;

        decimal grossLossPerPosition = _riskManagementOptions.Margin * (_riskManagementOptions.SLPercentages / 100m);

        if (_riskManagementOptions.GrossLossLimit < grossLossPerPosition)
            throw new InvalidRiskManagementException();

        // Task<IEnumerable<Position?>> closedPositionsForLossTask = _broker.GetClosedPositions(_time.GetUtcNow().Date);
        Task<IEnumerable<Position?>> closedPositionsForLossTask = _broker.GetClosedPositions(_time.GetUtcNow().Date.AddDays(-1).AddHours(6));
        // Task<IEnumerable<Position?>> closedPositionsForLossTask = _broker.GetClosedPositions(_time.GetUtcNow().AddSeconds(-1 * Math.Abs((double)_riskManagementOptions.GrossLossInterval)));
        Task<IEnumerable<Position?>> closedPositionsForProfitTask = _broker.GetClosedPositions(_time.GetUtcNow().AddSeconds(-1 * Math.Abs((double)_riskManagementOptions.GrossProfitInterval)));
        Task<IEnumerable<Position?>> openedPositionsTask = _broker.GetOpenPositions();
        await Task.WhenAll(closedPositionsForLossTask, closedPositionsForProfitTask, openedPositionsTask);
        IEnumerable<Position> closedPositionsForLoss = closedPositionsForLossTask.Result.Where(o => o != null)!;
        IEnumerable<Position> closedPositionsForProfit = closedPositionsForProfitTask.Result.Where(o => o != null)!;
        IEnumerable<Position> openedPositions = openedPositionsTask.Result.Where(o => o != null)!;

        if (_riskManagementOptions.NumberOfConcurrentPositions != 0 && openedPositions.Count() >= _riskManagementOptions.NumberOfConcurrentPositions)
            return false;

        if (_riskManagementOptions.GrossProfitInterval != 0)
        {
            decimal grossProfit = PnLAnalysis.PnLAnalysis.GetGrossProfit(closedPositionsForProfit);

            if (grossProfit >= _riskManagementOptions.GrossProfitLimit)
                return false;
        }

        if (_riskManagementOptions.GrossLossInterval != 0)
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
