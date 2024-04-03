namespace bot.src.Bots.UtBot.Models;

public class UtBotBotMessage : IUtBotMessage
{
    public string Id { get; set; } = null!;
    public string From { get; set; } = null!;
    public string Body { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public string Direction { get; set; } = null!;
    public bool OpeningPosition { get; set; }
    public decimal SlPrice { get; set; }
    public decimal? TpPrice { get; set; }
}
