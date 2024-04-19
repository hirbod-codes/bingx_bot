namespace Abstractions.src.Utilities;

public interface ITimeSimulator : ITime
{
    public void SetUtcNow(DateTime dateTime);
}

