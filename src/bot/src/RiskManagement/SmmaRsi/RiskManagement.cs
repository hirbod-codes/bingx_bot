using bot.src.Data.Models;

namespace bot.src.RiskManagement.SmmaRsi;

public class RiskManagement : IRiskManagement
{
    public RiskManagementOptions RiskManagementOptions { get; }

    public RiskManagement(IRiskManagementOptions riskManagementOptions) => RiskManagementOptions = (riskManagementOptions as RiskManagementOptions)!;

    public decimal GetLeverage() => RiskManagementOptions.Leverage;

    public decimal GetMargin() => RiskManagementOptions.Margin;

    public decimal GetSLPrice(string positionDirection, decimal positionEntryPrice)
    {
        decimal priceDifference = RiskManagementOptions.SLPercentages * positionEntryPrice / 100m / RiskManagementOptions.Leverage;
        if (positionDirection == PositionDirection.LONG)
            return positionEntryPrice - priceDifference;
        else
            return positionEntryPrice + priceDifference;
    }

    public decimal GetTPPrice(string positionDirection, decimal positionEntryPrice)
    {
        decimal priceDifference = RiskManagementOptions.SLPercentages * positionEntryPrice * RiskManagementOptions.Ratio / 100m / RiskManagementOptions.Leverage;
        if (positionDirection == PositionDirection.LONG)
            return positionEntryPrice + priceDifference;
        else
            return positionEntryPrice - priceDifference;
    }
}
