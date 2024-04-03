using System.Timers;
using bot.src.Util;

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

    public Task StartTimer(int interval, ElapsedEventHandler elapsedEventHandler)
    {
        throw new NotImplementedException();
    }
}
