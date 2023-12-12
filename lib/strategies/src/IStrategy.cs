namespace email_api.src;

public interface IStrategy
{
    public Task<bool> CheckClosePositionSignal(bool? isLastOpenPositionLong);
    public Task<bool> CheckOpenPositionSignal(bool? isLastOpenPositionLong);
    public ISignalProvider GetLastSignal();
    public Task Initiate();
}
