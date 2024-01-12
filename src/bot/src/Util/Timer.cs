using System.Timers;

namespace bot.src.Util;

public class Time : ITime
{
    public DateTime GetUtcNow() => DateTime.UtcNow;

    public async Task Sleep(int milliseconds) => await Task.Delay(milliseconds);

    public Task StartTimer(int interval, ElapsedEventHandler elapsedEventHandler)
    {

        System.Timers.Timer t = new()
        {
            AutoReset = false,
            Interval = GetInterval(interval)
        };

        t.Elapsed += new ElapsedEventHandler((o, args) =>
        {
            t.Interval = GetInterval(interval);
            t.Start();
        });

        t.Elapsed += elapsedEventHandler;

        t.Start();

        return Task.CompletedTask;
    }

    private static double GetInterval(int interval)
    {
        DateTime now = DateTime.UtcNow;
        return (interval - now.Second) * 1000 - now.Millisecond;
    }
}
