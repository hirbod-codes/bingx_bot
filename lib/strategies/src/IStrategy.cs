namespace email_api.src;

public interface IStrategy
{
    public Task<bool> CheckClosePositionSignal(bool? isLastOpenPositionLong, int timeFrame);
    public Task<bool> CheckOpenPositionSignal(bool? isLastOpenPositionLong, int timeFrame);
    public ISignalProvider GetLastSignal();
    public Task Initiate();
}
