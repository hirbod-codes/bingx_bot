namespace bot.src.Brokers.Bingx.Models;

public class BingxWsResponse
{
    public string Id { get; set; } = null!;
    public int Code { get; set; }
    public string? S { get; set; } = null!;
    public string? DataType { get; set; } = null!;
    public BingxWsCandle[]? Data { get; set; } = null!;
}
