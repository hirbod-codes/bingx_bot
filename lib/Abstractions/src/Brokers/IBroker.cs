using Abstractions.src.Data.Models;
using Abstractions.src.PnLAnalysis.Models;

namespace Abstractions.src.Brokers;

public interface IBroker
{
    public Task CandleClosed();
    public Task InitiateCandleStore(int? candlesCount = null, int? timeFrame = null);
    public Task<decimal> GetLastPrice();
    public Task<Candle?> GetCandle(int indexFromEnd = 0);
    public Task<Candles?> GetCandles(int? timeFrameSeconds = null);
    public Task<IEnumerable<Position?>> GetOpenPositions();
    public Task<IEnumerable<Position?>> GetClosedPositions(DateTime? start = null, DateTime? end = null);
    public Task<int?> GetLastCandleIndex();
    public Task<IEnumerable<Position?>> GetPendingPositions();
    public Task CloseAllPositions();
    public Task CancelPosition(string id, DateTime cancelledAt);
    public Task CancelAllPendingPositions();
    public Task ClosePosition(Position position);
    public Task CancelAllLongPendingPositions();
    public Task CancelAllShortPendingPositions();
    public Task OpenMarketPosition(decimal entryPrice, decimal margin, decimal leverage, string direction, decimal slPrice, decimal tpPrice);
    public Task OpenMarketPosition(decimal entryPrice, decimal margin, decimal leverage, string direction, decimal slPrice);
    public Task OpenLimitPosition(decimal entryPrice, decimal margin, decimal leverage, string direction, decimal limit, decimal slPrice);
    public Task OpenLimitPosition(decimal entryPrice, decimal margin, decimal leverage, string direction, decimal limit, decimal slPrice, decimal tpPrice);
    public void StopListening();
    public void StartListening();
    public Task<decimal> GetBalance();
    public Task<IEnumerable<PnlFundFlow>> GetPnlFundFlow();
    public Task<Asset> GetAssets();
}
