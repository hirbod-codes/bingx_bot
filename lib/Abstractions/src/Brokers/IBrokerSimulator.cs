namespace Abstractions.src.Brokers;

public interface IBrokerSimulator : IBroker
{
    public void NextCandle();
    public bool IsFinished();
}
