using bot.src.Strategies.Models;

namespace bot.src.Strategies.DoubleUtBot;

public class StrategyOptions : IStrategyOptions
{
    public string ProviderName { get; set; } = null!;
    public decimal SLDifference { get; set; }

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
    public int NaturalTrendIndicatorLength { get; set; } = 0;
    public decimal NaturalTrendIndicatorLimit { get; set; } = 0;
}
