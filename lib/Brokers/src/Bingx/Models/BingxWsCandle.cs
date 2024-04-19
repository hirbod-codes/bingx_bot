namespace Brokers.src.Bingx.Models;

public class BingxWsCandle
{
    public string c { get; set; } = null!;
    public string o { get; set; } = null!;
    public string h { get; set; } = null!;
    public string l { get; set; } = null!;
    public string v { get; set; } = null!;
    public long T { get; set; }
}
