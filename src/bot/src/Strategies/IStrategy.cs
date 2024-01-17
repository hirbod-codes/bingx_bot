using bot.src.Strategies.GeneralStrategy;

namespace bot.src.Strategies;

public interface IStrategy
{
    /// <exception cref="InvalidProviderException"></exception>
    /// <exception cref="InvalidSignalException"></exception>
    /// <exception cref="ExpiredSignalException"></exception>
    /// <exception cref="MessageParseException"></exception>
    public Task<bool> CheckForSignal();

    public string GetDirection();
    public decimal GetMargin();
    public decimal GetLeverage();
    public decimal GetSLPrice();

    /// <returns>Time frame in seconds</returns>
    public int GetTimeFrame();

    /// <returns>Determines the price in which the position will exit from. If null positions exit when a position in opposite direction is opened.</returns>
    public decimal? GetTPPrice();
    public bool IsParallelPositionsAllowed();
    public bool ShouldOpenPosition();
    public bool ShouldCloseAllPositions();
}
