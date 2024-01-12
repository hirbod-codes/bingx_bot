namespace bot.src.Brokers.Bingx.DTOs;

public class BingxLastPriceDto
{
    public string Symbol { get; set; } = null!;
    public string Price { get; set; } = null!;
    public string Time { get; set; } = null!;
}
