using bot.src.Bots;
using bot.src.Bots.SuperTrendV1;
using bot.src.Brokers;
using bot.src.Brokers.Bingx;
using bot.src.Data;
using bot.src.Indicators;
using bot.src.MessageStores;
using bot.src.Notifiers;
using bot.src.RiskManagement;
using bot.src.RiskManagement.SuperTrendV1;
using bot.src.Runners;
using bot.src.Runners.SuperTrendV1;
using bot.src.Strategies;
using bot.src.Strategies.SuperTrendV1;
using bot.src.Util;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Settings.Configuration;

namespace bot;

public class Program
{
    private static IConfigurationRoot _configuration = null!;
    private static ILogger _logger = null!;

    private static async Task Main(string[] args)
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        _logger = new LoggerConfiguration()
        .ReadFrom.Configuration(_configuration, new ConfigurationReaderOptions() { SectionName = ConfigurationKeys.SERILOG })
        .CreateLogger();



    start:

        ApplySettings(out IMessageStoreOptions messageStoreOptions, out IBrokerOptions brokerOptions, out IRiskManagementOptions riskManagementOptions, out IIndicatorOptions indicatorOptions, out IStrategyOptions strategyOptions, out IBotOptions botOptions, out IRunnerOptions runnerOptions, out IPositionRepository positionRepository, out IMessageRepository messageRepository, out INotifier notifier);
        try
        {
            try
            {
                ITime time = new Time();

                IBroker broker = BrokerFactory.CreateBroker(_configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, _logger, time);

                IMessageStore messageStore = MessageStoreFactory.CreateMessageStore(_configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!, messageStoreOptions, messageRepository, _logger);

                IRiskManagement riskManagement = RiskManagementFactory.CreateRiskManager(_configuration[ConfigurationKeys.RISK_MANAGEMENT_NAME]!, riskManagementOptions, broker, time, _logger);

                IStrategy strategy = StrategyFactory.CreateStrategy(_configuration[ConfigurationKeys.STRATEGY_NAME]!, strategyOptions, indicatorOptions, riskManagement, broker, notifier, messageRepository, _logger);

                IBot bot = BotFactory.CreateBot(_configuration[ConfigurationKeys.BOT_NAME]!, broker, botOptions, messageStore, riskManagement, time, notifier, _logger);

                IRunner runner = RunnerFactory.CreateRunner(_configuration[ConfigurationKeys.RUNNER_NAME]!, runnerOptions, bot, broker, strategy, time, notifier, _logger);

                await runner.Run();

                _logger.Information("Runner terminated at: {dateTime}", time.GetUtcNow().ToString());
                await notifier.SendMessage($"Runner terminated at: {time.GetUtcNow()}");
            }
            catch (System.Exception ex)
            {
                _logger.Error(ex, "An unhandled exception has been thrown.");
                try
                {
                    _logger.Information("Notifying listeners...");
                    await notifier.SendMessage($"Unhandled exception was thrown: {ex.Message}");
                    _logger.Information("Listeners are notified.");
                }
                catch (System.Exception)
                {
                    _logger.Error(ex, "Failed to notify listeners.");
                    throw;
                }

                _logger.Information("Restarting...");
                goto start;
            }
        }
        catch (System.Exception ex)
        {
            _logger.Information("Runner terminated with a fatal error at: {dateTime}", DateTime.UtcNow.ToString());
            _logger.Error(ex, "An unhandled exception has been thrown.");
            return;
        }
    }

    private static void ApplySettings(out IMessageStoreOptions messageStoreOptions, out IBrokerOptions brokerOptions, out IRiskManagementOptions riskManagementOptions, out IIndicatorOptions indicatorOptions, out IStrategyOptions strategyOptions, out IBotOptions botOptions, out IRunnerOptions runnerOptions, out IPositionRepository positionRepository, out IMessageRepository messageRepository, out INotifier notifier)
    {
        _configuration[ConfigurationKeys.MESSAGE_STORE_NAME] = MessageStoreNames.IN_MEMORY;
        messageStoreOptions = MessageStoreOptionsFactory.CreateMessageStoreOptions(MessageStoreNames.IN_MEMORY);
        _logger.Information("messageStoreOptions: {@messageStoreOptions}", messageStoreOptions);

        _configuration[ConfigurationKeys.BROKER_NAME] = BrokerNames.BINGX;
        brokerOptions = BrokerOptionsFactory.CreateBrokerOptions(BrokerNames.BINGX);
        (brokerOptions as BrokerOptions)!.ApiKey = "ce7YRR5dNQwhPnGjTxbKm8y9ArYGtfi9V7gh4qYxebYTZtOjY49MxkD3al76uKoFtxoTVpfr84z11J2KRxAOw";
        (brokerOptions as BrokerOptions)!.ApiSecret = "GjBZow7grcgclbKQMZHikttkMWDiUfdtZNOdjc7vHIoNu7egNfSvjGBOwCS6MvTgt5dl2zV34NsmrCr8PRyw";
        (brokerOptions as BrokerOptions)!.BaseUrl = "open-api-vst.bingx.com";
        (brokerOptions as BrokerOptions)!.BrokerCommission = 0.001m;
        (brokerOptions as BrokerOptions)!.Symbol = "BTC-USDT";
        (brokerOptions as BrokerOptions)!.TimeFrame = 3600;
        _logger.Information("brokerOptions: {@brokerOptions}", brokerOptions);

        _configuration[ConfigurationKeys.RISK_MANAGEMENT_NAME] = RiskManagementNames.SUPER_TREND_V1;
        riskManagementOptions = RiskManagementOptionsFactory.RiskManagementOptions(RiskManagementNames.SUPER_TREND_V1);
        (riskManagementOptions as RiskManagementOptions)!.BrokerCommission = 0.001m;
        (riskManagementOptions as RiskManagementOptions)!.BrokerMaximumLeverage = 100;
        (riskManagementOptions as RiskManagementOptions)!.CommissionPercentage = 100;
        (riskManagementOptions as RiskManagementOptions)!.GrossLossLimit = 0;
        (riskManagementOptions as RiskManagementOptions)!.GrossProfitLimit = 0;
        (riskManagementOptions as RiskManagementOptions)!.Margin = 100;
        (riskManagementOptions as RiskManagementOptions)!.NumberOfConcurrentPositions = 0;
        (riskManagementOptions as RiskManagementOptions)!.RiskRewardRatio = 2;
        (riskManagementOptions as RiskManagementOptions)!.SLPercentages = 12.5m;
        _logger.Information("riskManagementOptions: {@riskManagementOptions}", riskManagementOptions);

        indicatorOptions = IndicatorOptionsFactory.CreateIndicatorOptions(IndicatorsOptionsNames.SUPER_TREND_V1);
        _logger.Information("indicatorOptions: {@indicatorOptions}", indicatorOptions);

        _configuration[ConfigurationKeys.STRATEGY_NAME] = StrategyNames.SUPER_TREND_V1;
        strategyOptions = StrategyOptionsFactory.CreateStrategyOptions(StrategyNames.SUPER_TREND_V1);
        (strategyOptions as StrategyOptions)!.RiskRewardRatio = 2;
        _logger.Information("strategyOptions: {@strategyOptions}", strategyOptions);

        _configuration[ConfigurationKeys.BOT_NAME] = BotNames.SUPER_TREND_V1;
        botOptions = BotOptionsFactory.CreateBotOptions(BotNames.SUPER_TREND_V1);
        (botOptions as BotOptions)!.TimeFrame = 3600;
        _logger.Information("botOptions: {@botOptions}", botOptions);

        _configuration[ConfigurationKeys.RUNNER_NAME] = RunnerNames.SUPER_TREND_V1;
        runnerOptions = RunnerOptionsFactory.CreateRunnerOptions(RunnerNames.SUPER_TREND_V1);
        (runnerOptions as RunnerOptions)!.HistoricalCandlesCount = 5000;
        (runnerOptions as RunnerOptions)!.TimeFrame = 3600;
        _logger.Information("runnerOptions: {@runnerOptions}", runnerOptions);

        positionRepository = PositionRepositoryFactory.CreateRepository(PositionRepositoryNames.IN_MEMORY);
        _logger.Information("positionRepository: {@positionRepository}", positionRepository);

        messageRepository = MessageRepositoryFactory.CreateRepository(MessageRepositoryNames.IN_MEMORY);
        _logger.Information("messageRepository: {@messageRepository}", messageRepository);

        notifier = NotifierFactory.CreateNotifier(NotifierNames.IN_MEMORY, messageRepository, _logger);
        _logger.Information("notifier: {@notifier}", notifier);
    }
}
