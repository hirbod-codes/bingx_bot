namespace broker_api.src;

public interface IMarket
{
    public Task<float> GetLastPrice(string symbol, int timeFrame);
}
