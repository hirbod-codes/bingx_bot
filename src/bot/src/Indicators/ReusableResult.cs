using Skender.Stock.Indicators;

namespace bot.src.Indicators;

public class ReusableResult : IReusableResult
{
    public double? Value { get; set; }

    public DateTime Date { get; set; }
}
