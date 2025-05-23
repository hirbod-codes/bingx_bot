namespace Brokers.src.Bingx;

public interface IBingxUtilities
{
    public Task<HttpResponseMessage> HandleBingxRequest(string protocol, string host, string endpointAddress, string method, string apiKey, string apiSecret, object payload);
    public Task EnsureSuccessfulBingxResponse(HttpResponseMessage response);
    public Task<bool> TryEnsureSuccessfulBingxResponse(HttpResponseMessage response);
    public Task<byte[]> DecompressBytes(byte[] bytes);
    public Task<byte[]> DecompressBytesAsync(byte[] bytes, CancellationToken cancel = default);
}
