namespace Abstractions.src.Bots;

public interface IBot
{
    public Task Run();
    public Task Tick();
}
