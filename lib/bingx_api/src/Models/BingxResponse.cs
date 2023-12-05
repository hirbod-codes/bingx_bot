namespace bingx_api.src.Models;

public class BingxResponse<T>
{
    public int Code { get; set; }
    public T? Data { get; set; }
    public string Msg { get; set; } = null!;
}

public class BingxResponse
{
    public int Code { get; set; }
    public object Data { get; set; } = null!;
    public string Msg { get; set; } = null!;
}
