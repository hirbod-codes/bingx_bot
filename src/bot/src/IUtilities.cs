
namespace bot.src;

public interface IUtilities
{
    public Task Sleep(int millisecondsDelay);
    public bool IsTerminationDatePassed(DateTime? terminationDate);
    public bool HasTimeFrameReached(int timeFrame);
}
