namespace email_api.src;

public interface ISignalProvider
{
    public Task<bool> CheckSignals();
    public int GetLeverage();
    public float GetMargin();
    public float GetSLPrice();
    public DateTime GetSignalTime();
    public float? GetTPPrice();
    public Task Initiate();
    public bool IsSignalLong();
    public void ResetSignals();
}
