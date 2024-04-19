namespace Abstractions.src.Brokers;

public interface IBrokerOptions : IEquatable<IBrokerOptions>
{
    public int TimeFrame { get; set; }
    public new bool Equals(IBrokerOptions? other);
}
