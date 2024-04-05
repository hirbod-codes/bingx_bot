using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Data;
using bot.src.Indicators;
using bot.src.MessageStores;
using bot.src.Notifiers;
using bot.src.RiskManagement;
using bot.src.Runners;
using bot.src.Strategies;
using bot.src.Util;
using ILogger = Serilog.ILogger;
using Serilog.Settings.Configuration;
using Serilog;
using bot.src.Configuration.Providers.DockerSecrets;
using bot.src.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace bot;

public class Program
{
    public const string ENV_PREFIX = "BOT_";
    public static string RootPath { get; set; } = "";

    private static ConfigurationManager _configuration = null!;
    private static ILogger _logger = null!;

    private static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        _configuration = builder.Configuration;

        RootPath = builder.Environment.ContentRootPath;

        _configuration.AddJsonFile("appsettings.json");
        _configuration.AddEnvironmentVariables(ENV_PREFIX);

        _logger.Information("Environment: {Environment}", builder.Environment.EnvironmentName);

        if (builder.Environment.IsProduction())
            _configuration.AddDockerSecrets(allowedPrefixesCommaDelimited: _configuration["SECRETS_PREFIX"]);
        else
            _configuration.AddJsonFile("appsettings.development.json");

        _logger = new LoggerConfiguration()
            .ReadFrom.Configuration(_configuration, new ConfigurationReaderOptions() { SectionName = ConfigurationKeys.SERILOG })
            .CreateLogger();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApiDocument(config =>
        {
            config.DocumentName = "TodoAPI";
            config.Title = "TodoAPI v1";
            config.Version = "v1";
        });

        var app = builder.Build();

        OptionsNames.PositionRepositoryName = _configuration[ConfigurationKeys.POSITION_REPOSITORY_NAME];
        OptionsNames.MessageRepositoryName = _configuration[ConfigurationKeys.MESSAGE_REPOSITORY_NAME];
        OptionsNames.NotifierName = _configuration[ConfigurationKeys.NOTIFIER_NAME];
        OptionsNames.MessageStoreName = _configuration[ConfigurationKeys.MESSAGE_STORE_NAME];
        OptionsNames.BrokerName = _configuration[ConfigurationKeys.BROKER_NAME];
        OptionsNames.RiskManagementName = _configuration[ConfigurationKeys.RISK_MANAGEMENT_NAME];
        OptionsNames.StrategyName = _configuration[ConfigurationKeys.STRATEGY_NAME];
        OptionsNames.IndicatorOptionsName = _configuration[ConfigurationKeys.INDICATOR_OPTIONS_NAME];
        OptionsNames.BotName = _configuration[ConfigurationKeys.BOT_NAME];
        OptionsNames.RunnerName = _configuration[ConfigurationKeys.RUNNER_NAME];

        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUi(config =>
            {
                config.DocumentTitle = "Bot";
                config.Path = "/swagger";
                config.DocumentPath = "/swagger/{documentName}/swagger.json";
                config.DocExpansion = "list";
            });
        }

        Options? options = new();
        Services? services = null;

        if (app.Environment.IsDevelopment())
            options = Options.ApplyDefaults();

        async Task WaitForRunnerToTick()
        {
            long timeFrame = options.TimeFrame ?? 60;
            DateTime dt = DateTime.UtcNow;
            do
            {
                await Task.Delay(1000);
                dt = dt.AddSeconds(1);
            } while (DateTimeOffset.Parse(dt.ToString()).ToUnixTimeSeconds() % timeFrame < 7);
        }

        // ------------------------------------------------------------------------------------------------ Options

        app.MapGet("/options", () => Results.Ok(
            new
            {
                BotOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.BotOptions, OptionsNames.BotOptionsType!), OptionsNames.BotOptionsType!),
                RunnerOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.RunnerOptions, OptionsNames.RunnerOptionsType!), OptionsNames.RunnerOptionsType!),
                StrategyOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.StrategyOptions, OptionsNames.StrategyOptionsType!), OptionsNames.StrategyOptionsType!),
                IndicatorOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.IndicatorOptions, OptionsNames.IndicatorOptionsType!), OptionsNames.IndicatorOptionsType!),
                RiskManagementOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.RiskManagementOptions, OptionsNames.RiskManagementOptionsType!), OptionsNames.RiskManagementOptionsType!),
                BrokerOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.BrokerOptions, OptionsNames.BrokerOptionsType!), OptionsNames.BrokerOptionsType!)
            }
        ));

        app.MapPatch("/bot-options", async (JsonNode opt) =>
        {
            try
            {
                await WaitForRunnerToTick();

                string json = JsonSerializer.Serialize(opt);
                options.BotOptions = (IBotOptions)opt.Deserialize(OptionsNames.BotOptionsType!)!;

                return Results.Ok();
            }
            catch (Exception) { return Results.Problem("Server failed to process your request."); }
        });

        app.MapPatch("/runner-options", async (JsonNode opt) =>
        {
            try
            {
                await WaitForRunnerToTick();

                string json = JsonSerializer.Serialize(opt);
                options.RunnerOptions = (IRunnerOptions)opt.Deserialize(OptionsNames.RunnerOptionsType!)!;

                return Results.Ok();
            }
            catch (Exception) { return Results.Problem("Server failed to process your request."); }
        });

        app.MapPatch("/strategy-options", async (JsonNode opt) =>
        {
            try
            {
                await WaitForRunnerToTick();

                string json = JsonSerializer.Serialize(opt);
                options.StrategyOptions = (IStrategyOptions)opt.Deserialize(OptionsNames.StrategyOptionsType!)!;

                return Results.Ok();
            }
            catch (Exception) { return Results.Problem("Server failed to process your request."); }
        });

        app.MapPatch("/indicator-options", async (JsonNode opt) =>
        {
            try
            {
                await WaitForRunnerToTick();

                string json = JsonSerializer.Serialize(opt);
                options.IndicatorOptions = (IIndicatorOptions)opt.Deserialize(OptionsNames.IndicatorOptionsType!)!;

                return Results.Ok();
            }
            catch (Exception) { return Results.Problem("Server failed to process your request."); }
        });

        app.MapPatch("/risk-management-options", async (JsonNode opt) =>
        {
            try
            {
                await WaitForRunnerToTick();

                string json = JsonSerializer.Serialize(opt);
                options.RiskManagementOptions = (IRiskManagementOptions)opt.Deserialize(OptionsNames.RiskManagementOptionsType!)!;

                return Results.Ok();
            }
            catch (Exception) { return Results.Problem("Server failed to process your request."); }
        });

        app.MapPatch("/broker-options", async (JsonNode opt) =>
        {
            try
            {
                await WaitForRunnerToTick();

                string json = JsonSerializer.Serialize(opt);
                options.BrokerOptions = (IBrokerOptions)opt.Deserialize(OptionsNames.BrokerOptionsType!)!;

                return Results.Ok();
            }
            catch (Exception) { return Results.Problem("Server failed to process your request."); }
        });

        // ------------------------------------------------------------------------------------------------ Status

        app.MapGet("/status", () =>
        {
            if (services == null || services.Runner == null)
                return Results.BadRequest(new { Message = "You have not started any bots yet." });
            else
                return Results.Ok(new { Status = services.Runner!.Status.ToString() });
        });
        app.MapPost("/start", async () =>
        {
            if (services == null)
                try { services = CreateServices(options!); }
                catch (Exception) { return Results.BadRequest(new { Message = "We failed to create the bot, invalid options provided." }); }

            if (services!.Runner == null)
                return Results.BadRequest(new { Message = "You have not set any options yet." });

            await services.Runner!.Continue();
            return Results.Ok();
        });
        app.MapPost("/suspend", async () =>
        {
            if (services == null || services.Runner == null)
                return Results.BadRequest(new { Message = "You have not started any bots yet." });

            await services.Runner!.Suspend();
            return Results.Ok();
        });
        app.MapPost("/stop", async () =>
        {
            if (services == null || services.Runner == null)
                return Results.BadRequest(new { Message = "You have not started any bots yet." });

            await services.Runner!.Stop();
            return Results.Ok();
        });

        app.Run();
    }

    private static Services? CreateServices(Options options)
    {
        Services services = new();

        services.PositionRepository = PositionRepositoryFactory.CreateRepository(OptionsNames.PositionRepositoryName!);

        services.MessageRepository = MessageRepositoryFactory.CreateRepository(OptionsNames.MessageRepositoryName!);

        services.Notifier = NotifierFactory.CreateNotifier(OptionsNames.NotifierName!, services.MessageRepository, _logger);

        services.Time = new Time();

        services.Broker = BrokerFactory.CreateBroker(OptionsNames.BrokerName!, options.BrokerOptions!, _logger, services.Time);

        services.MessageStore = MessageStoreFactory.CreateMessageStore(OptionsNames.MessageStoreName!, options.MessageStoreOptions!, services.MessageRepository, _logger);

        services.RiskManagement = RiskManagementFactory.CreateRiskManager(OptionsNames.RiskManagementName!, options.RiskManagementOptions!, services.Broker, services.Time, _logger);

        services.Strategy = StrategyFactory.CreateStrategy(OptionsNames.StrategyName!, options.StrategyOptions!, options.IndicatorOptions!, services.RiskManagement, services.Broker, services.Notifier!, services.MessageRepository!, _logger);

        services.Bot = BotFactory.CreateBot(OptionsNames.BotName!, services.Broker, options.BotOptions!, services.MessageStore, services.RiskManagement, services.Time, services.Notifier!, _logger);

        services.Runner = RunnerFactory.CreateRunner(OptionsNames.RunnerName!, options.RunnerOptions!, services.Bot, services.Broker, services.Strategy, services.Time, services.Notifier!, _logger);

        return services;
    }
}
