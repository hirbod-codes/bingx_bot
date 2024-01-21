namespace bot.src.Indicators.Models;

public class SmaValue : IValue
{
    public decimal? Value { get; set; }
    public DateTime Date { get; set; }
}
