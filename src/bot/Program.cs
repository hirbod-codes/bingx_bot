using bot.src.Bots;
using bot.src.Bots.SuperTrendV1;
using bot.src.Brokers;
using bot.src.Brokers.Bingx;
using bot.src.Data;
using bot.src.Indicators;
using bot.src.Indicators.SuperTrendV1;
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
    private static ILogger _logger = null!;

    private static async Task Main(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        _logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration, new ConfigurationReaderOptions() { SectionName = ConfigurationKeys.SERILOG })
        .CreateLogger();



    start:

        try
        {
            ApplySettings(out IMessageStoreOptions messageStoreOptions, out IBrokerOptions brokerOptions, out IRiskManagementOptions riskManagementOptions, out IIndicatorOptions indicatorOptions, out IStrategyOptions strategyOptions, out IBotOptions botOptions, out IRunnerOptions runnerOptions, out IPositionRepository positionRepository, out IMessageRepository messageRepository, out INotifier notifier);
            try
            {
                ITime time = new Time();

                IBroker broker = BrokerFactory.CreateBroker(configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, _logger, time);

                IMessageStore messageStore = MessageStoreFactory.CreateMessageStore(configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!, messageStoreOptions, messageRepository, _logger);

                IRiskManagement riskManagement = RiskManagementFactory.CreateRiskManager(configuration[ConfigurationKeys.RISK_MANAGEMENT_NAME]!, riskManagementOptions, broker, time, _logger);

                IStrategy strategy = StrategyFactory.CreateStrategy(configuration[ConfigurationKeys.STRATEGY_NAME]!, strategyOptions, indicatorOptions, riskManagement, broker, notifier, messageRepository, _logger);

                IBot bot = BotFactory.CreateBot(configuration[ConfigurationKeys.BOT_NAME]!, broker, botOptions, messageStore, riskManagement, time, notifier, _logger);

                IRunner runner = RunnerFactory.CreateRunner(configuration[ConfigurationKeys.RUNNER_NAME]!, runnerOptions, bot, broker, strategy, time, notifier, _logger);

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
        messageStoreOptions = MessageStoreOptionsFactory.CreateMessageStoreOptions(MessageStoreNames.IN_MEMORY);

        brokerOptions = BrokerOptionsFactory.CreateBrokerOptions(BrokerNames.BINGX);
        (brokerOptions as BrokerOptions)!.ApiKey = "ce7YRR5dNQwhPnGjTxbKm8y9ArYGtfi9V7gh4qYxebYTZtOjY49MxkD3al76uKoFtxoTVpfr84z11J2KRxAOw";
        (brokerOptions as BrokerOptions)!.ApiSecret = "GjBZow7grcgclbKQMZHikttkMWDiUfdtZNOdjc7vHIoNu7egNfSvjGBOwCS6MvTgt5dl2zV34NsmrCr8PRyw";
        (brokerOptions as BrokerOptions)!.BaseUrl = "open-api-vst.bingx.com";
        (brokerOptions as BrokerOptions)!.BrokerCommission = 0.001m;
        (brokerOptions as BrokerOptions)!.Symbol = "BTC-USDT";
        (brokerOptions as BrokerOptions)!.TimeFrame = 3600;

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

        indicatorOptions = IndicatorOptionsFactory.CreateIndicatorOptions(IndicatorsOptionsNames.SUPER_TREND_V1);

        strategyOptions = StrategyOptionsFactory.CreateStrategyOptions(StrategyNames.SUPER_TREND_V1);
        (strategyOptions as StrategyOptions)!.RiskRewardRatio = 2;

        botOptions = BotOptionsFactory.CreateBotOptions(BotNames.SUPER_TREND_V1);
        (botOptions as BotOptions)!.TimeFrame = 3600;

        runnerOptions = RunnerOptionsFactory.CreateRunnerOptions(RunnerNames.SUPER_TREND_V1);
        (runnerOptions as RunnerOptions)!.HistoricalCandlesCount = 5000;
        (runnerOptions as RunnerOptions)!.TimeFrame = 3600;

        positionRepository = PositionRepositoryFactory.CreateRepository(PositionRepositoryNames.IN_MEMORY);
        messageRepository = MessageRepositoryFactory.CreateRepository(MessageRepositoryNames.IN_MEMORY);

        notifier = NotifierFactory.CreateNotifier(NotifierNames.IN_MEMORY, messageRepository, _logger);
    }
}
