namespace bot.src.Data.Models;

public class Position
{
    public string Id { get; set; } = null!;
    public string Symbol { get; set; } = null!;
    public decimal OpenedPrice { get; set; }
    public decimal? ClosedPrice { get; set; }
    public decimal SLPrice { get; set; }
    public decimal? TPPrice { get; set; }
    public decimal CommissionRatio { get; set; }
    public decimal? Commission { get; set; }
    public decimal? Profit { get; set; }
    public decimal? ProfitWithCommission { get; set; }
    public decimal Margin { get; set; }
    public decimal Leverage { get; set; }
    private string _positionStatus = Models.PositionStatus.OPENED;
    public string PositionStatus
    {
        get { return _positionStatus!; }
        set
        {
            if (value == Models.PositionStatus.OPENED || value == Models.PositionStatus.CLOSED || value == Models.PositionStatus.PENDING || value == Models.PositionStatus.CANCELLED)
                _positionStatus = value;
            else
                throw new ArgumentException("Invalid value provided", paramName: nameof(PositionDirection));
        }
    }
    private string? _positionDirection;
    public string PositionDirection
    {
        get { return _positionDirection!; }
        set
        {
            if (value == Models.PositionDirection.LONG || value == Models.PositionDirection.SHORT)
                _positionDirection = value;
            else
                throw new ArgumentException("Invalid value provided", paramName: nameof(PositionDirection));
        }
    }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}
