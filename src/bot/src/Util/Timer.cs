using System.Timers;

namespace bot.src.Util;

public class Time : ITime
{
    public DateTime GetUtcNow() => DateTime.UtcNow;

    public async Task Sleep(int milliseconds) => await Task.Delay(milliseconds);

    public Task StartTimer(int interval, ElapsedEventHandler elapsedEventHandler)
    {
        System.Timers.Timer timer = new()
        {
            Enabled = true,
            AutoReset = false,
            Interval = GetInterval(interval)
        };

        timer.Elapsed += elapsedEventHandler;

        timer.Elapsed += new ElapsedEventHandler((o, args) =>
        {
            timer.Interval = GetInterval(interval);
            timer.Start();
        });

        timer.Start();

        return Task.CompletedTask;
    }

    private static double GetInterval(int interval)
    {
        DateTime now = DateTime.UtcNow;
        DateTime tempNow = new(now.Ticks, DateTimeKind.Utc);

        while (DateTimeOffset.Parse(tempNow.ToString()).ToUnixTimeSeconds() % interval != 0)
            tempNow = tempNow.AddSeconds(1);

        tempNow = tempNow.AddMilliseconds(-1 * tempNow.Millisecond);

        double totalMilliseconds = (tempNow - now).TotalMilliseconds;

        if (totalMilliseconds <= 0)
            return (interval * 1000) - now.Millisecond;
        else
            return totalMilliseconds;
    }
}
