using bot.src.Strategies.Models;

namespace bot.src.Strategies.SuperTrendV1;

public class StrategyOptions : IStrategyOptions
{
    public string ProviderName { get; set; } = StrategyNames.SUPER_TREND_V1;
    /// <summary>
    /// Can be left empty.
    /// </summary>
    public IEnumerable<DayOfWeek> InvalidWeekDays { get; set; } = Array.Empty<DayOfWeek>();
    /// <summary>
    /// Can be left empty.
    /// </summary>
    public IEnumerable<DateTimePeriod> InvalidTimePeriods { get; set; } = Array.Empty<DateTimePeriod>();
    /// <summary>
    /// Can be left empty.
    /// </summary>
    public IEnumerable<DateTimePeriod> InvalidDatePeriods { get; set; } = Array.Empty<DateTimePeriod>();
}
