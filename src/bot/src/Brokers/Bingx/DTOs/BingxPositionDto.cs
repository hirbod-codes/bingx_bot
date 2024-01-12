namespace bot.src.Brokers.Bingx.DTOs;

public class BingxPositionDto
{
    public int Time { get; set; }
    public string Symbol { get; set; } = null!;
    public string Side { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string PositionSide { get; set; } = null!;
    public string ReduceOnly { get; set; } = null!;
    public string CumQuote { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string StopPrice { get; set; } = null!;
    public string Price { get; set; } = null!;
    public string OrigQty { get; set; } = null!;
    public string AvgPrice { get; set; } = null!;
    public string ExecutedQty { get; set; } = null!;
    public int OrderId { get; set; }
    public string Profit { get; set; } = null!;
    public string Commission { get; set; } = null!;
    public int UpdateTime { get; set; }
    public string WorkingType { get; set; } = null!;
    public string ClientOrderID { get; set; } = null!;
}
