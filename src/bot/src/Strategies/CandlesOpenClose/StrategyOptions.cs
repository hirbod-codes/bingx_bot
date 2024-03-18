using bot.src.Strategies.Models;

namespace bot.src.Strategies.CandlesOpenClose;

public class StrategyOptions : IStrategyOptions
{
    public string ProviderName { get; set; } = null!;
    public decimal RiskRewardRatio { get; set; } = 2;
    /// <summary>
    /// Can be left empty.
    /// </summary>
    public IEnumerable<DayOfWeek> InvalidWeekDays { get; set; } = Array.Empty<DayOfWeek>();
    // public IEnumerable<DayOfWeek> InvalidWeekDays { get; set; } = new DayOfWeek[] { DayOfWeek.Saturday, DayOfWeek.Sunday };
    /// <summary>
    /// Can be left empty.
    /// </summary>
    public IEnumerable<DateTimePeriod> InvalidTimePeriods { get; set; } = Array.Empty<DateTimePeriod>();
    /// <summary>
    /// Can be left empty.
    /// </summary>
    public IEnumerable<DateTimePeriod> InvalidDatePeriods { get; set; } = Array.Empty<DateTimePeriod>();
}
