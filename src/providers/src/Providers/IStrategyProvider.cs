namespace providers.src.Providers;

public interface IStrategyProvider
{
    public Task Reset();
    public Task MoveToNextCandle();
}
