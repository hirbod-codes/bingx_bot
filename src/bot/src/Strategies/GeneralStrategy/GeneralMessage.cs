using bot.src.MessageStores;

namespace bot.src.Strategies.GeneralStrategy;

public class GeneralMessage : IGeneralMessage
{
    public bool AllowingParallelPositions { get; set; }
    public bool ClosingAllPositions { get; set; }
    public string Direction { get; set; } = null!;
    public decimal Leverage { get; set; }
    public decimal Margin { get; set; }
    public bool OpeningPosition { get; set; }
    public decimal SlPrice { get; set; }
    public int TimeFrame { get; set; }
    public decimal? TpPrice { get; set; }
    public string Id { get; set; } = null!;
    public string From { get; set; } = null!;
    public string Body { get; set; } = null!;
    public DateTime SentAt { get; set; }
}
