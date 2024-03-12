namespace bot.src.RiskManagement.UtBot;

public class RiskManagement : IRiskManagement
{
    private readonly RiskManagementOptions _riskManagementOptions;

    public RiskManagement(IRiskManagementOptions riskManagementOptions) => _riskManagementOptions = (riskManagementOptions as RiskManagementOptions)!;

    public decimal CalculateLeverage(decimal entryPrice, decimal slPrice) => _riskManagementOptions.SLPercentages * entryPrice / (100m * Math.Abs(entryPrice - slPrice));

    public decimal CalculateTpPrice(decimal leverage, decimal entryPrice, string direction)
    {
        throw new NotImplementedException();
    }

    public decimal GetMargin() => _riskManagementOptions.Margin;

    public decimal GetMarginRelativeToLimitedLeverage(decimal entryPrice, decimal slPrice)
    {
        throw new NotImplementedException();
    }

    public Task<bool> PermitOpenPosition(decimal entryPrice, decimal slPrice) => Task.FromResult(true);
}
