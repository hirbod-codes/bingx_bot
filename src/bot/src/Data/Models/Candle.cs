using Skender.Stock.Indicators;

namespace bot.src.Data.Models;

public class Candle : IQuote, ISeries
{
    public decimal Open { get; set; }
    public decimal Close { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public DateTime Date { get; set; }
    public decimal Volume { get; set; }

    public decimal GetSource(string source) => source switch
    {
        CandleSources.OPEN => Open,
        CandleSources.CLOSE => Close,
        CandleSources.HIGH => High,
        CandleSources.LOW => Low,
        _ => throw new ArgumentException("Invalid source provided for candle source.")
    };
}
