namespace bot.src.Bots;

public interface IBot
{
    public event EventHandler? Ticked;
    public Task Run();
    public Task Tick();
}
