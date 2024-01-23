namespace bot.src.RiskManagement;

public interface IRiskManagement
{
    public decimal GetLeverage(decimal entryPrice, decimal slPrice);
    public decimal GetMargin();
    public Task<bool> PermitOpenPosition();
}
