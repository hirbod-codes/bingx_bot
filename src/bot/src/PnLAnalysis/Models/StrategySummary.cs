namespace bot.src.PnLAnalysis.Models;

public class StrategySummary
{
    public decimal HighestNetProfit { get; set; }
    public decimal HighestDrawDown { get; set; }
    public decimal DraDown { get; set; }
    public decimal NetProfit { get; set; }
    public decimal LongGrossProfit { get; set; }
    public decimal ShortGrossProfit { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal LongGrossLoss { get; set; }
    public decimal ShortGrossLoss { get; set; }
    public decimal GrossLoss { get; set; }
}
