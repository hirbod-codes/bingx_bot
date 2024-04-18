using System.Timers;
using Abstractions.src.Utilities;
using Timer = System.Timers.Timer;

namespace Util.src;

public class Time : ITime
{
    public DateTime GetUtcNow() => DateTime.UtcNow;

    public async Task Sleep(int milliseconds) => await Task.Delay(milliseconds);

    public Task<Timer> StartTimer(int secondsInterval, ElapsedEventHandler elapsedEventHandler, int millisecondsOffset = 0)
    {
        if (secondsInterval * 1000 <= millisecondsOffset)
            throw new ArgumentException(message: $"{nameof(millisecondsOffset)} must be lower then {nameof(secondsInterval)}", nameof(millisecondsOffset));

        Timer timer = new()
        {
            Enabled = true,
            AutoReset = false,
            Interval = GetInterval(secondsInterval, millisecondsOffset)
        };

        timer.Elapsed += new ElapsedEventHandler((o, args) =>
        {
            timer.Interval = GetInterval(secondsInterval, millisecondsOffset);
            timer.Start();
        });

        timer.Elapsed += elapsedEventHandler;

        timer.Start();

        return Task.FromResult(timer);
    }

    private static double GetInterval(int secondsInterval, int millisecondsOffset)
    {
        DateTime now = DateTime.UtcNow;
        DateTime tempNow = new(now.Ticks, DateTimeKind.Utc);

        while (DateTimeOffset.Parse(tempNow.ToString()).ToUnixTimeSeconds() % secondsInterval != 0)
            tempNow = tempNow.AddSeconds(1);

        tempNow = tempNow.AddMilliseconds(-1 * tempNow.Millisecond);

        double totalMilliseconds = (tempNow - now).TotalMilliseconds;

        if (totalMilliseconds <= 0)
            return (secondsInterval * 1000) - now.Millisecond - millisecondsOffset;
        else if (totalMilliseconds > millisecondsOffset)
            return totalMilliseconds - millisecondsOffset;
        else
            return (secondsInterval * 1000) - now.Millisecond - millisecondsOffset + totalMilliseconds;
    }
}
