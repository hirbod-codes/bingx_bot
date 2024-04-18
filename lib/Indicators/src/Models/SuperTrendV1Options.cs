using Abstractions.src.Indicators.Models;

namespace Indicators.src.Models;

public class SuperTrendV1Options
{
    public int Period { get; set; }
    public double Multiplier { get; set; }
    public CandlePart CandlePart { get; set; }
    public bool ChangeATRCalculationMethod { get; set; }
}
