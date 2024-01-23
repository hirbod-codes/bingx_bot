using bot.src.MessageStores;

namespace bot.src.Bots.General.Models;

public class GeneralBotMessage : IGeneralMessage
{
    public string Id { get; set; } = null!;
    public string From { get; set; } = null!;
    public string Body { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public bool AllowingParallelPositions { get; set; }
    public bool ClosingAllPositions { get; set; }
    public string Direction { get; set; } = null!;
    public bool OpeningPosition { get; set; }
    public decimal SlPrice { get; set; }
    public decimal? TpPrice { get; set; }
}
