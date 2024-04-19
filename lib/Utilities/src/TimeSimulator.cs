using System.Timers;
using Abstractions.src.Utilities;
using Timer = System.Timers.Timer;

namespace Utilities.src;

public class TimeSimulator : ITimeSimulator
{
    private DateTime _dateTime;

    public void SetUtcNow(DateTime dateTime) => _dateTime = dateTime;

    public DateTime GetUtcNow() => _dateTime;

    public Task Sleep(int milliseconds)
    {
        throw new NotImplementedException();
    }

    public Task<Timer> StartTimer(int secondsInterval, ElapsedEventHandler elapsedEventHandler, int millisecondsOffset = 0)
    {
        throw new NotImplementedException();
    }
}
