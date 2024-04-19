using Abstractions.src.RiskManagement;

namespace RiskManagement.src;

public static class RiskManagementOptionsFactory
{
    public static IRiskManagementOptions RiskManagementOptions(string riskManagementName) => riskManagementName switch
    {
        RiskManagementNames.SUPER_TREND_V1 => new SuperTrendV1.RiskManagementOptions(),
        _ => throw new Exception()
    };

    public static Type? GetInstanceType(string? name) => name switch
    {
        null => null,
        RiskManagementNames.SUPER_TREND_V1 => typeof(SuperTrendV1.RiskManagementOptions),
        _ => throw new Exception()
    };
}
