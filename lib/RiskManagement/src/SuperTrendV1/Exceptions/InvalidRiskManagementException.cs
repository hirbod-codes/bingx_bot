using System.Runtime.Serialization;
using Abstractions.src.Strategies.Exceptions;

namespace RiskManagement.src.SuperTrendV1.Exceptions;

[Serializable]
public class InvalidRiskManagementException : StrategyException
{
    public InvalidRiskManagementException() { }

    public InvalidRiskManagementException(string message) : base(message) { }

    public InvalidRiskManagementException(string message, StrategyException innerException) : base(message, innerException) { }

    protected InvalidRiskManagementException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
