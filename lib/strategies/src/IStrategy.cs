namespace strategies.src;

public interface IStrategy
{
    public Task Initiate();
    public bool CheckClosePositionSignal(bool? IsCurrentOpenPositionLong);
    public bool? CheckOpenPositionSignal(bool? IsCurrentOpenPositionLong);
}
