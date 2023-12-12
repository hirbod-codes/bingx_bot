namespace broker_api.src.Models;

public class Order
{
    public string Symbol { get; set; } = null!;
    public long OrderId { get; set; }
    public string Side { get; set; } = null!;
    public string PositionSide { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Price { get; set; } = null!;
    public string Profit { get; set; } = null!;
    public string Commission { get; set; } = null!;
    public string Status { get; set; } = null!;
    public long Time { get; set; }
    public long UpdateTime { get; set; }
}
