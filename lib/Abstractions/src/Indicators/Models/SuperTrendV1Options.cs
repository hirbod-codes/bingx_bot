namespace Abstractions.src.Indicators.Models;

public class SuperTrendV1Options
{
    public int Period { get; set; }
    public double Multiplier { get; set; }
    public CandlePart CandlePart { get; set; }
    public bool ChangeATRCalculationMethod { get; set; }
}
