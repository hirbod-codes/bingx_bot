using Abstractions.src.Brokers;
using Abstractions.src.Data.Models;
using Abstractions.src.PnLAnalysis;
using Abstractions.src.RiskManagement;
using Abstractions.src.Utilities;
using RiskManagement.src.SuperTrendV1.Exceptions;
using ILogger = Serilog.ILogger;

namespace RiskManagement.src.SuperTrendV1;

public class RiskManagement : IRiskManagement
{
    private readonly RiskManagementOptions _riskManagementOptions;
    private readonly IBroker _broker;
    private readonly ITime _time;
    private readonly ILogger _logger;

    public RiskManagement(IRiskManagementOptions riskManagementOptions, IBroker broker, ITime time, ILogger logger)
    {
        _riskManagementOptions = (riskManagementOptions as RiskManagementOptions)!;
        _broker = broker;
        _time = time;
        _logger = logger.ForContext<RiskManagement>();
    }

    private decimal GetMaximumLeverage() => _riskManagementOptions.BrokerMaximumLeverage;

    public int GetUnacceptableOrdersCount() => 0;

    public decimal CalculateLeverage(decimal entryPrice, decimal slPrice) => _riskManagementOptions.Leverage;

    private decimal CalculateDelta(decimal leverage, decimal entryPrice) => _riskManagementOptions.SLPercentages * entryPrice / 100.0m / leverage;

    public decimal CalculateTpPrice(decimal leverage, decimal entryPrice, string direction)
    {
        decimal delta = _riskManagementOptions.RiskRewardRatio * CalculateDelta(leverage, entryPrice);
        _logger.Debug("leverage: {leverage}, entryPrice: {entryPrice}, direction: {direction}, delta: {delta}", leverage, entryPrice, direction, delta);

        if (direction == PositionDirection.LONG)
            return entryPrice + delta;
        else
            return entryPrice - delta;
    }

    public decimal CalculateSlPrice(decimal leverage, decimal entryPrice, string direction)
    {
        decimal delta = CalculateDelta(leverage, entryPrice);
        _logger.Debug("leverage: {leverage}, entryPrice: {entryPrice}, direction: {direction}, delta: {delta}", leverage, entryPrice, direction, delta);

        if (direction == PositionDirection.LONG)
            return entryPrice - delta;
        else
            return entryPrice + delta;
    }

    public decimal GetMargin() => _riskManagementOptions.Margin;

    public decimal GetMarginRelativeToLimitedLeverage(decimal entryPrice, decimal slPrice) => throw new NotImplementedException();

    public async Task<bool> IsPositionAcceptable(decimal entryPrice, decimal slPrice)
    {
        if (GetMaximumLeverage() < CalculateLeverage(entryPrice, slPrice))
        {
            _logger.Information("Risk Management rejecting: Calculated Leverage exceeded maximum acceptable leverage. Calculated Leverage: {calculatedLeverage}, Maximum leverage: {maximumLeverage}", CalculateLeverage(entryPrice, slPrice), GetMaximumLeverage());
            return false;
        }

        if (_riskManagementOptions.GrossProfitLimit == 0 && _riskManagementOptions.GrossLossLimit == 0 && _riskManagementOptions.NumberOfConcurrentPositions == 0)
            return true;

        decimal grossLossPerPosition = _riskManagementOptions.Margin * (_riskManagementOptions.SLPercentages / 100m);

        if (_riskManagementOptions.GrossLossLimit < grossLossPerPosition)
            throw new InvalidRiskManagementException();

        Task<IEnumerable<Position?>> closedPositionsForLossTask = _broker.GetClosedPositions();
        Task<IEnumerable<Position?>> closedPositionsForProfitTask = _broker.GetClosedPositions();
        Task<IEnumerable<Position?>> openedPositionsTask = _broker.GetOpenPositions();
        await Task.WhenAll(closedPositionsForLossTask, closedPositionsForProfitTask, openedPositionsTask);
        IEnumerable<Position> closedPositionsForLoss = closedPositionsForLossTask.Result.Where(o => o != null)!;
        IEnumerable<Position> closedPositionsForProfit = closedPositionsForProfitTask.Result.Where(o => o != null)!;
        IEnumerable<Position> openedPositions = openedPositionsTask.Result.Where(o => o != null)!;

        if (_riskManagementOptions.NumberOfConcurrentPositions != 0 && openedPositions.Count() >= _riskManagementOptions.NumberOfConcurrentPositions)
        {
            _logger.Information("Risk Management rejecting: Number of concurrent positions exceeded the provided limit. NumberOfConcurrentPositions: {NumberOfConcurrentPositions}, ConcurrentPositionsLimit: {ConcurrentPositionsLimit}", openedPositions.Count(), _riskManagementOptions.NumberOfConcurrentPositions);
            return false;
        }

        if (_riskManagementOptions.GrossLossLimit == 0 && _riskManagementOptions.GrossProfitLimit == 0)
            return true;

        // Adding another grossLossPerPosition because of current possible position
        decimal openedPositionsMaximumLoss = (openedPositions.Count() * grossLossPerPosition) + grossLossPerPosition;

        if (_riskManagementOptions.GrossLossLimit != 0 && openedPositionsMaximumLoss >= _riskManagementOptions.GrossLossLimit)
        {
            _logger.Information("Risk Management rejecting: Gross loss limit has been exceeded.");
            return false;
        }

        decimal grossLoss = PnLAnalysis.GetGrossLoss(closedPositionsForLoss) + openedPositionsMaximumLoss;

        if (_riskManagementOptions.GrossLossLimit != 0 && grossLoss >= Math.Abs(_riskManagementOptions.GrossLossLimit))
        {
            _logger.Information("Risk Management rejecting: Gross loss limit has been exceeded.");
            return false;
        }

        decimal grossProfit = Math.Abs(grossLoss - PnLAnalysis.GetGrossProfit(closedPositionsForProfit));

        if (_riskManagementOptions.GrossProfitLimit != 0 && grossProfit >= _riskManagementOptions.GrossProfitLimit)
        {
            _logger.Information("Risk Management rejecting: Gross profit limit has been exceeded.");
            return false;
        }

        return true;
    }
}
