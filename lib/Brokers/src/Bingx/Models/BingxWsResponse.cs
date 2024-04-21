namespace Brokers.src.Bingx.Models;

public class BingxWsResponse
{
    public string Id { get; set; } = null!;
    public int Code { get; set; }
    public BingxWsCandle[]? Data { get; set; } = null!;
}
