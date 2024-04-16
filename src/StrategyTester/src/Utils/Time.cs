using System.Timers;
using bot.src.Util;
using Timer = System.Timers.Timer;

namespace StrategyTester.src.Utils;

public class Time : ITime
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
