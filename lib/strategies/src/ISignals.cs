namespace strategies.src;

public interface ISignals
{
    public Task Initiate();
    public bool CheckShortSignal();
    public bool CheckLongSignal();
}
