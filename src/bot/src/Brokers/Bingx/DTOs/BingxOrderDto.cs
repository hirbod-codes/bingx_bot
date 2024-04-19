namespace bot.src.Brokers.Bingx.DTOs;

public class BingxOrdersDto
{
    public IEnumerable<BingxOrderDto> Orders { get; set; } = Array.Empty<BingxOrderDto>();
}

public class BingxOrderDto
{
    public string Symbol { get; set; } = null!;
    public long OrderId { get; set; }
    public string Side { get; set; } = null!;
    public string PositionSide { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string OrigQty { get; set; } = null!;
    public string Price { get; set; } = null!;
    public string ExecutedQty { get; set; } = null!;
    public string AvgPrice { get; set; } = null!;
    public string CumQuote { get; set; } = null!;
    public string StopPrice { get; set; } = null!;
    public string Profit { get; set; } = null!;
    public string Commission { get; set; } = null!;
    public string Status { get; set; } = null!;
    public long Time { get; set; }
    public long UpdateTime { get; set; }
    public string ClientOrderId { get; set; } = null!;
    public string Leverage { get; set; } = null!;
    public string WorkingType { get; set; } = null!;
    public bool ReduceOnly { get; set; }
}