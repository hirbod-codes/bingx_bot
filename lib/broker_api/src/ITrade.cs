namespace broker_api.src;

public interface ITrade
{
    public string GetSymbol();
    public Task<HttpResponseMessage> GetLeverage();
    public Task<HttpResponseMessage> SetLeverage(int leverage, bool isLong);
    public Task<HttpResponseMessage> OpenMarketOrder(bool isLong, float quantity, float? tp, float sl);
    public Task<HttpResponseMessage> OpenLimitOrder(bool isLong, float quantity, float price, float tp, float sl);
    public Task<HttpResponseMessage> OpenTriggerLimitOrder(bool isLong, float quantity, float price, float stopPrice, float tp, float sl);
    public Task<HttpResponseMessage> OpenTriggerMarketOrder(bool isLong, float quantity, float stopPrice, float tp, float sl);
    public Task<HttpResponseMessage> GetOrders();
    public Task<HttpResponseMessage> GetOrders(DateTime startTime, DateTime endTime);
    public Task<HttpResponseMessage> CloseOpenPositions();
    public Task<HttpResponseMessage> CloseOrders();
    public Task<HttpResponseMessage> CloseOrders(IEnumerable<long> orderIds);
    public Task<HttpResponseMessage> CloseOrder(long orderId);
}
