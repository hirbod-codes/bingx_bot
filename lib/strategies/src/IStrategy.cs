namespace strategies.src;

public interface IStrategy
{
    public Task Initiate();
    public bool CheckClosePositionSignal(bool? isLastOpenPositionLong);
    public bool? CheckOpenPositionSignal(bool? isLastOpenPositionLong);
}
