namespace providers.src;

public interface IRiskManagement
{
    public decimal GetLeverage();
    public decimal GetMargin();
    public decimal GetSLPrice(string positionDirection, decimal positionEntryPrice);
    public decimal GetTPPrice(string positionDirection, decimal positionEntryPrice);
}
