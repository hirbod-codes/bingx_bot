using System.Timers;

namespace bot.src.Util;

public interface ITime
{
    public DateTime GetUtcNow();
    public Task Sleep(int milliseconds);
    public Task StartTimer(int interval, ElapsedEventHandler elapsedEventHandler);
}
