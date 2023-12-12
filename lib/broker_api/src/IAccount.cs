namespace broker_api.src;

public interface IAccount
{
    public Task<HttpResponseMessage> GetBalance();
    public Task<int> GetOpenPositionCount();
}
