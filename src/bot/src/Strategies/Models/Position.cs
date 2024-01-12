namespace bot.src.Strategies.Models;

public class Position
{
    public float EntryPrice { get; set; }
    public float SLPrice { get; set; }
    public float TPPrice { get; set; }
    public float Commission { get; set; }
    public float Leverage { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}
