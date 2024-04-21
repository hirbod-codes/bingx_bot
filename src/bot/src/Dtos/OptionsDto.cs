namespace bot.src.Dtos;

public class OptionsDto
{
    public object? MessageStoreOptions { get; set; }
    public object? BrokerOptions { get; set; }
    public object? RiskManagementOptions { get; set; }
    public object? IndicatorOptions { get; set; }
    public object? StrategyOptions { get; set; }
    public object? BotOptions { get; set; }
    public object? RunnerOptions { get; set; }
}
