namespace bot.src.RiskManagement.DoubleUtBot;

public class RiskManagement : IRiskManagement
{
    private readonly RiskManagementOptions _riskManagementOptions;

    public RiskManagement(IRiskManagementOptions riskManagementOptions) => _riskManagementOptions = (riskManagementOptions as RiskManagementOptions)!;

    public decimal GetLeverage(decimal entryPrice, decimal slPrice) => _riskManagementOptions.SLPercentages * entryPrice / (100m * Math.Abs(entryPrice - slPrice));

    public decimal GetMargin() => _riskManagementOptions.Margin;

    public Task<bool> PermitOpenPosition() => Task.FromResult(true);
}
