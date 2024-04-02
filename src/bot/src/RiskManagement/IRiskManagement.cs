namespace bot.src.RiskManagement;

public interface IRiskManagement
{
    public int GetUnacceptableOrdersCount();
    public decimal CalculateLeverage(decimal entryPrice, decimal slPrice);
    public decimal CalculateTpPrice(decimal leverage, decimal entryPrice, string direction);
    public decimal GetMargin();
    public decimal GetMarginRelativeToLimitedLeverage(decimal entryPrice, decimal slPrice);
    public Task<bool> IsPositionAcceptable(decimal entryPrice, decimal slPrice);
    decimal CalculateSlPrice(decimal leverage, decimal entryPrice, string direction);
}
