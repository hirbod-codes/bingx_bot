using System.Timers;

namespace Abstractions.src.Utilities;

public interface ITime
{
    public DateTime GetUtcNow();
    public Task Sleep(int milliseconds);
    public Task<System.Timers.Timer> StartTimer(int secondsInterval, ElapsedEventHandler elapsedEventHandler, int millisecondsOffset = 0);
}
