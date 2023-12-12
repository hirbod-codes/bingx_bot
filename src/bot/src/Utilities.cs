
namespace bot.src;

public class Utilities : IUtilities
{
    public async Task Sleep(int millisecondsDelay)
    {
        Program.Logger.Information("Waiting {milliseconds} milliseconds...", millisecondsDelay);
        await Task.Delay(millisecondsDelay);
    }

    public bool IsTerminationDatePassed(DateTime? terminationDate) => terminationDate is null || (terminationDate is not null && DateTime.UtcNow < terminationDate);

    public bool HasTimeFrameReached(int timeFrame) => DateTime.UtcNow.Minute % timeFrame != 0 || DateTime.UtcNow.Second != 0;
}
