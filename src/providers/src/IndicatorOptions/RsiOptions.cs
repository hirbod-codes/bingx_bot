namespace providers.src.IndicatorOptions;

public class RsiOptions
{
    public int Period { get; set; }
    public string Source { get; set; } = null!;
    public int UpperBand { get; set; }
    public int LowerBand { get; set; }
}
