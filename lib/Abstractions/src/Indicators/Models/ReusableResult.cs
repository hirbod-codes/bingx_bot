using Skender.Stock.Indicators;

namespace Abstractions.src.Indicators.Models;

public class ReusableResult : IReusableResult
{
    public double? Value { get; set; }

    public DateTime Date { get; set; }
}
