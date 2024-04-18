using System.Runtime.Serialization;

namespace RiskManagement.src.SuperTrendV1.Exceptions;

[Serializable]
public class InvalidRiskManagementException : Exception
{
    public InvalidRiskManagementException() { }

    public InvalidRiskManagementException(string message) : base(message) { }

    public InvalidRiskManagementException(string message, Exception innerException) : base(message, innerException) { }

    protected InvalidRiskManagementException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
