namespace bot.src.Indicators.Models;

public interface IValue
{
    public decimal? Value { get; set; }
    public DateTime Date { get; set; }
}
