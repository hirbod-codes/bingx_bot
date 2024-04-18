using bot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Settings.Configuration;
using StrategyTester.src.Testers;
using System.Text.Json;
using Serilog.Core;
using StrategyTester.Dtos;
using Serilog;
using Abstractions.src.Brokers;
using Notifiers.src;
using RiskManagement.src;
using Strategies.src;
using Bots.src;
using Abstractions.src.Bots;
using Abstractions.src.Strategies;
using Abstractions.src.RiskManagements;
using Abstractions.src.Notifiers;
using Abstractions.src.MessageStore;
using MessageStores.src;
using Repositories.src;
using Abstractions.src.Repository;
using Indicators.src;
using PnLAnalysis.src.Models;
using Abstractions.src.Utilities;
using Util.src;
using Brokers.src;

namespace StrategyTester;

public class Program
{
    private static IConfigurationRoot _configuration = null!;
    private static Logger _logger = null!;

    private static async Task Main(string[] args)
    {
        string rootDirectory = Directory.GetCurrentDirectory();

        string strategyName = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText($"{rootDirectory}{Path.DirectorySeparatorChar}appsettings.json"))!.GetValueOrDefault(ConfigurationKeys.STRATEGY_NAME).GetString()!;

        string scenariosDirectory = $"{rootDirectory}{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}Scenarios";
        string strategyDirectory = $"{rootDirectory}{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}Scenarios{Path.DirectorySeparatorChar}{strategyName}";

        string strategyJsonFileName = $"{strategyDirectory}{Path.DirectorySeparatorChar}scenarios.json";
        object[] scenarios = JsonSerializer.Deserialize<object[]>(File.ReadAllText(strategyJsonFileName)) ?? throw new Exception("System failed to parse scenarios json file.");

        string logFile = $"{strategyDirectory}{Path.DirectorySeparatorChar}result.log";
        string resultsJsonFile = $"{strategyDirectory}{Path.DirectorySeparatorChar}results.json";
        string resultsJsFile = $"{strategyDirectory}{Path.DirectorySeparatorChar}results.js";

        try
        {
            await File.WriteAllTextAsync(logFile, String.Empty);
            await File.AppendAllTextAsync(logFile, $"{scenarios.Length} scenarios are parsed to be tested at {DateTime.UtcNow}.");

            List<Result> strategyTestResults = new();
            int y = 0;
            for (; y < scenarios.Length; y++)
            {
                object scenario = scenarios[y];

                _configuration = null!;
                _logger = null!;

                _configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddJsonStream(GenerateStreamFromString(JsonSerializer.Serialize(scenario)))
                    .AddEnvironmentVariables()
                    .AddCommandLine(args)
                    .Build();

                _logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(_configuration, new ConfigurationReaderOptions() { SectionName = ConfigurationKeys.SERILOG })
                    .CreateLogger();

                try { await RunScenario(strategyTestResults); }
                catch (Exception ex)
                {
                    await File.AppendAllTextAsync(logFile, $"\nScenario with index:{y} has thrown an exception at {DateTime.UtcNow}, with message {ex.Message}, Skipping");
                    throw;
                    // continue;
                }

                await File.AppendAllTextAsync(logFile, $"\nScenario {y} has completed at {DateTime.UtcNow}.");
            }

            await File.AppendAllTextAsync(logFile, $"\n{y} scenarios has completed successfully at {DateTime.UtcNow}.");

            string serializedStrategyTestResults = JsonSerializer.Serialize(strategyTestResults, new JsonSerializerOptions()
            {
                IncludeFields = true,
                WriteIndented = true,
                UnknownTypeHandling = System.Text.Json.Serialization.JsonUnknownTypeHandling.JsonElement
            });

            await File.WriteAllTextAsync(resultsJsonFile, serializedStrategyTestResults);
            await File.WriteAllTextAsync(resultsJsFile, $"var {strategyName}Results = " + serializedStrategyTestResults);

            string[] scenariosDirectoryEntries = Directory.EnumerateDirectories($"{scenariosDirectory}").ToArray();

            Dictionary<string, Result[]> strategiesResults = new();
            for (int i = 0; i < scenariosDirectoryEntries.Length; i++)
            {
                string path = $"{scenariosDirectoryEntries[i]}{Path.DirectorySeparatorChar}results.json";

                if (!File.Exists(path))
                    continue;

                Result[] strategyResults = JsonSerializer.Deserialize<Result[]>(await File.ReadAllTextAsync(path))!;

                strategiesResults.Add(scenariosDirectoryEntries[i].Split(Path.DirectorySeparatorChar).Last(), strategyResults);
            }

            await File.WriteAllTextAsync($"{scenariosDirectory}{Path.DirectorySeparatorChar}results.json", JsonSerializer.Serialize(strategiesResults, new JsonSerializerOptions()
            {
                WriteIndented = true,
                UnknownTypeHandling = System.Text.Json.Serialization.JsonUnknownTypeHandling.JsonElement
            }));
            await File.WriteAllTextAsync($"{scenariosDirectory}{Path.DirectorySeparatorChar}results.js", "var results = " + JsonSerializer.Serialize(strategiesResults, new JsonSerializerOptions()
            {
                WriteIndented = true,
                UnknownTypeHandling = System.Text.Json.Serialization.JsonUnknownTypeHandling.JsonElement
            }));
        }
        catch (System.Exception ex) { await File.AppendAllTextAsync(logFile, $"An unhandled exception was thrown: {ex.Message}"); throw; }
    }

    private static Stream GenerateStreamFromString(string str)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(str);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    private static async Task RunScenario(List<Result> results)
    {
        var notifierOptions = NotifierOptionsFactory.CreateNotifierOptions(_configuration[ConfigurationKeys.NOTIFIER_NAME]!);
        _configuration.Bind($"{ConfigurationKeys.NOTIFIER_OPTIONS}", notifierOptions);

        var messageStoreOptions = MessageStoreOptionsFactory.CreateMessageStoreOptions(_configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!);
        _configuration.Bind($"{ConfigurationKeys.MESSAGE_STORE_OPTIONS}", messageStoreOptions);

        var brokerOptions = BrokerOptionsFactory.CreateBrokerOptions(_configuration[ConfigurationKeys.BROKER_NAME]!);
        _configuration.Bind($"{ConfigurationKeys.BROKER_OPTIONS}", brokerOptions);

        var botOptions = BotOptionsFactory.CreateBotOptions(_configuration[ConfigurationKeys.BOT_NAME]!);
        _configuration.Bind($"{ConfigurationKeys.BOT_OPTIONS}", botOptions);

        var riskManagementOptions = RiskManagementOptionsFactory.RiskManagementOptions(_configuration[ConfigurationKeys.RISK_MANAGEMENT_NAME]!);
        _configuration.Bind($"{ConfigurationKeys.RISK_MANAGEMENT_OPTIONS}", riskManagementOptions);

        var indicatorsOptions = IndicatorOptionsFactory.CreateIndicatorOptions(_configuration[ConfigurationKeys.INDICATOR_OPTIONS_NAME]!);
        _configuration.Bind($"{ConfigurationKeys.INDICATOR_OPTIONS}", indicatorsOptions);

        var strategyOptions = StrategyOptionsFactory.CreateStrategyOptions(_configuration[ConfigurationKeys.STRATEGY_NAME]!);
        _configuration.Bind($"{ConfigurationKeys.STRATEGY_OPTIONS}", strategyOptions);

        var testerOptions = TesterOptionsFactory.CreateTesterOptions(_configuration[ConfigurationKeys.TESTER_NAME]!);
        _configuration.Bind($"{ConfigurationKeys.TESTER_OPTIONS}", testerOptions);

        ICandleRepository candleRepository = CandleRepositoryFactory.CreateRepository(_configuration[ConfigurationKeys.CANDLE_REPOSITORY_NAME]!);
        IPositionRepository positionRepository = PositionRepositoryFactory.CreateRepository(_configuration[ConfigurationKeys.POSITION_REPOSITORY_NAME]!);
        IMessageRepository messageRepository = MessageRepositoryFactory.CreateRepository(_configuration[ConfigurationKeys.MESSAGE_REPOSITORY_NAME]!);

        IMessageStore messageStore = MessageStoreFactory.CreateMessageStore(_configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!, messageStoreOptions, messageRepository, _logger);

        ITime time = new TimeSimulator() as ITime;

        IBrokerSimulator broker = (BrokerFactory.CreateBroker(_configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, _logger, time, candleRepository, positionRepository) as IBrokerSimulator)!;

        INotifier notifier = NotifierFactory.CreateNotifier(_configuration[ConfigurationKeys.NOTIFIER_NAME]!, messageRepository, _logger, notifierOptions);

        IRiskManagement riskManagement = RiskManagementFactory.CreateRiskManager(_configuration[ConfigurationKeys.RISK_MANAGEMENT_NAME]!, riskManagementOptions, broker, time, _logger);

        IStrategy strategy = StrategyFactory.CreateStrategy(_configuration[ConfigurationKeys.STRATEGY_NAME]!, strategyOptions, indicatorsOptions, broker, notifier, messageRepository, _logger);

        IBot bot = BotFactory.CreateBot(_configuration[ConfigurationKeys.BOT_NAME]!, broker, botOptions, messageStore, riskManagement, time, notifier, _logger);

        ITester tester = TesterFactory.CreateTester(_configuration[ConfigurationKeys.TESTER_NAME]!, testerOptions, time, strategy, broker, bot, _logger);

        await tester.Test();

        AnalysisSummary analysisSummary = await PnLAnalysis.src.PnLAnalysis.RunAnalysis(positionRepository, messageRepository, strategy.GetIndicators(), riskManagement, broker);

        results.Add(new Result
        {
            PnlResults = analysisSummary,
            MessageStoreOptions = messageStoreOptions,
            BrokerOptions = brokerOptions,
            BotOptions = botOptions,
            RiskManagementOptions = riskManagementOptions,
            IndicatorsOptions = indicatorsOptions,
            StrategyOptions = strategyOptions,
            TesterOptions = testerOptions,
            ClosedPositions = (await positionRepository.GetClosedPositions()).Where(o => o != null)!
        });
    }
}
