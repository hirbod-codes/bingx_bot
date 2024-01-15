namespace bot.src;

public interface IBot
{
    public Task Run();
    public Task Tick();
}
