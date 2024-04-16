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
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using bot.src.Authentication.ApiKey;

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

        if (builder.Environment.IsProduction())
            _configuration.AddDockerSecrets(allowedPrefixesCommaDelimited: _configuration["SECRETS_PREFIX"]);
        else
            _configuration.AddJsonFile("appsettings.development.json");

        _logger = new LoggerConfiguration()
            .ReadFrom.Configuration(_configuration, new ConfigurationReaderOptions() { SectionName = ConfigurationKeys.SERILOG })
            .CreateLogger();

        _logger.Information("Environment: {Environment}", builder.Environment.EnvironmentName);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApiDocument(config =>
        {
            config.DocumentName = "TodoAPI";
            config.Title = "TodoAPI v1";
            config.Version = "v1";
        });

        builder.Services.AddCors((corsOptions) =>
        {
            corsOptions.DefaultPolicyName = "General-Cors";

            corsOptions.AddPolicy("General-Cors", (cpb) =>
            {
                cpb.AllowAnyHeader();
                cpb.AllowAnyMethod();
                cpb.SetIsOriginAllowed(o => true);
                cpb.AllowCredentials();
            });
        });

        builder.Services.AddRateLimiter(_ => _
            .AddFixedWindowLimiter(policyName: "fixed", options =>
            {
                options.PermitLimit = 20;
                options.Window = TimeSpan.FromSeconds(1);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 10;
            }));

        builder.Services.AddAuthentication("ApiKey")
            .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationSchemeHandler>("ApiKey", (opt) => opt.ApiKey = _configuration[ConfigurationKeys.API_KEY]!);
        builder.Services.AddAuthorization();

        var app = builder.Build();

        app.UseRateLimiter();

        app.UseCors("General-Cors");

        app.UseAuthentication();
        app.UseAuthorization();

        OptionsMetaData.PositionRepositoryName = _configuration[ConfigurationKeys.POSITION_REPOSITORY_NAME];
        OptionsMetaData.MessageRepositoryName = _configuration[ConfigurationKeys.MESSAGE_REPOSITORY_NAME];
        OptionsMetaData.NotifierName = _configuration[ConfigurationKeys.NOTIFIER_NAME];
        OptionsMetaData.MessageStoreName = _configuration[ConfigurationKeys.MESSAGE_STORE_NAME];
        OptionsMetaData.BrokerName = _configuration[ConfigurationKeys.BROKER_NAME];
        OptionsMetaData.RiskManagementName = _configuration[ConfigurationKeys.RISK_MANAGEMENT_NAME];
        OptionsMetaData.StrategyName = _configuration[ConfigurationKeys.STRATEGY_NAME];
        OptionsMetaData.IndicatorOptionsName = _configuration[ConfigurationKeys.INDICATOR_OPTIONS_NAME];
        OptionsMetaData.BotName = _configuration[ConfigurationKeys.BOT_NAME];
        OptionsMetaData.RunnerName = _configuration[ConfigurationKeys.RUNNER_NAME];

        _logger.Debug("ASPNETCORE_ENVIRONMENT: {ASPNETCORE_ENVIRONMENT}", _configuration["ASPNETCORE_ENVIRONMENT"]);
        _logger.Debug("ENVIRONMENT: {ENVIRONMENT}", _configuration["ENVIRONMENT"]);
        _logger.Debug("SECRETS_PREFIX: {SECRETS_PREFIX}", _configuration["SECRETS_PREFIX"]);
        _logger.Debug("Serilog:WriteTo:1:Args:serverUrl: {Serilog:WriteTo:1:Args:serverUrl}", _configuration["Serilog:WriteTo:1:Args:serverUrl"]);
        _logger.Debug("PositionRepositoryName: {PositionRepositoryName}", OptionsMetaData.PositionRepositoryName);
        _logger.Debug("MessageRepositoryName: {MessageRepositoryName}", OptionsMetaData.MessageRepositoryName);
        _logger.Debug("NotifierName: {NotifierName}", OptionsMetaData.NotifierName);
        _logger.Debug("MessageStoreName: {MessageStoreName}", OptionsMetaData.MessageStoreName);
        _logger.Debug("BrokerName: {BrokerName}", OptionsMetaData.BrokerName);
        _logger.Debug("RiskManagementName: {RiskManagementName}", OptionsMetaData.RiskManagementName);
        _logger.Debug("StrategyName: {StrategyName}", OptionsMetaData.StrategyName);
        _logger.Debug("IndicatorOptionsName: {IndicatorOptionsName}", OptionsMetaData.IndicatorOptionsName);
        _logger.Debug("BotName: {BotName}", OptionsMetaData.BotName);
        _logger.Debug("RunnerName: {RunnerName}", OptionsMetaData.RunnerName);

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
                BotOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.BotOptions, OptionsMetaData.BotOptionsType!), OptionsMetaData.BotOptionsType!),
                RunnerOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.RunnerOptions, OptionsMetaData.RunnerOptionsType!), OptionsMetaData.RunnerOptionsType!),
                StrategyOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.StrategyOptions, OptionsMetaData.StrategyOptionsType!), OptionsMetaData.StrategyOptionsType!),
                IndicatorOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.IndicatorOptions, OptionsMetaData.IndicatorOptionsType!), OptionsMetaData.IndicatorOptionsType!),
                RiskManagementOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.RiskManagementOptions, OptionsMetaData.RiskManagementOptionsType!), OptionsMetaData.RiskManagementOptionsType!),
                BrokerOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.BrokerOptions, OptionsMetaData.BrokerOptionsType!), OptionsMetaData.BrokerOptionsType!)
            }
        )).RequireAuthorization().RequireCors("General-Cors");

        app.MapPatch("/bot-options", async (JsonNode opt) =>
        {
            try
            {
                await WaitForRunnerToTick();

                string json = JsonSerializer.Serialize(opt);
                options.BotOptions = (IBotOptions)opt.Deserialize(OptionsMetaData.BotOptionsType!, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;

                return Results.Ok(options.BotOptions);
            }
            catch (Exception) { return Results.Problem("Server failed to process your request."); }
        }).RequireAuthorization().RequireCors("General-Cors");

        app.MapPatch("/runner-options", async (JsonNode opt) =>
        {
            try
            {
                await WaitForRunnerToTick();

                string json = JsonSerializer.Serialize(opt);
                options.RunnerOptions = (IRunnerOptions)opt.Deserialize(OptionsMetaData.RunnerOptionsType!, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;

                return Results.Ok(options.RunnerOptions);
            }
            catch (Exception) { return Results.Problem("Server failed to process your request."); }
        }).RequireAuthorization().RequireCors("General-Cors");

        app.MapPatch("/strategy-options", async (JsonNode opt) =>
        {
            try
            {
                await WaitForRunnerToTick();

                string json = JsonSerializer.Serialize(opt);
                options.StrategyOptions = (IStrategyOptions)opt.Deserialize(OptionsMetaData.StrategyOptionsType!, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;

                return Results.Ok(options.StrategyOptions);
            }
            catch (Exception) { return Results.Problem("Server failed to process your request."); }
        }).RequireAuthorization().RequireCors("General-Cors");

        app.MapPatch("/indicator-options", async (JsonNode opt) =>
        {
            try
            {
                await WaitForRunnerToTick();

                string json = JsonSerializer.Serialize(opt);
                options.IndicatorOptions = (IIndicatorOptions)opt.Deserialize(OptionsMetaData.IndicatorOptionsType!, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;

                return Results.Ok(options.IndicatorOptions);
            }
            catch (Exception) { return Results.Problem("Server failed to process your request."); }
        }).RequireAuthorization().RequireCors("General-Cors");

        app.MapPatch("/risk-management-options", async (JsonNode opt) =>
        {
            try
            {
                await WaitForRunnerToTick();

                string json = JsonSerializer.Serialize(opt);
                options.RiskManagementOptions = (IRiskManagementOptions)opt.Deserialize(OptionsMetaData.RiskManagementOptionsType!, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;

                return Results.Ok(options.RiskManagementOptions);
            }
            catch (Exception) { return Results.Problem("Server failed to process your request."); }
        }).RequireAuthorization().RequireCors("General-Cors");

        app.MapPatch("/broker-options", async (JsonNode opt) =>
        {
            try
            {
                await WaitForRunnerToTick();

                IBrokerOptions? oldOptionsBrokerOptions = options.BrokerOptions;

                string json = JsonSerializer.Serialize(opt);
                IBrokerOptions input = (IBrokerOptions)opt.Deserialize(OptionsMetaData.BrokerOptionsType!, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;

                if (options.BrokerOptions == null)
                {
                    options.BrokerOptions = input;
                    return Results.Ok(options.BrokerOptions);
                }

                if (options.BrokerOptions.Equals(input))
                    return Results.Ok(options.BrokerOptions);

                if (input.TimeFrame != options.BrokerOptions.TimeFrame)
                    options.BrokerOptions.TimeFrame = input.TimeFrame;

                if (options.BrokerOptions.Equals(input))
                    return Results.Ok(options.BrokerOptions);

                string previousStatus = services?.Runner!.Status.ToString() ?? RunnerStatus.STOPPED.ToString();
                Stop();
                options.BrokerOptions = input;
                try
                {
                    services = CreateServices(options!);
                    if (previousStatus == RunnerStatus.RUNNING.ToString())
                        Start();
                    if (previousStatus == RunnerStatus.SUSPENDED.ToString())
                        Suspend();
                }
                catch (Exception)
                {
                    options.BrokerOptions = oldOptionsBrokerOptions;
                    return Results.BadRequest(new { Message = $"We failed to update broker options, invalid options provided. Bot Status is: {RunnerStatus.STOPPED}" });
                }

                return Results.Ok(options.BrokerOptions);
            }
            catch (Exception) { return Results.Problem("Server failed to process your request."); }
        }).RequireAuthorization().RequireCors("General-Cors");

        // ------------------------------------------------------------------------------------------------ Status

        app.MapGet("/status", () =>
        {
            return Results.Ok(new { Status = services?.Runner!.Status.ToString() ?? RunnerStatus.STOPPED.ToString() });
        }).RequireAuthorization().RequireCors("General-Cors");

        app.MapPost("/start", Start()).RequireAuthorization().RequireCors("General-Cors");
        Func<Task<IResult>> Start() => async () =>
        {
            if (services == null)
                try { services = CreateServices(options!); }
                catch (Exception) { return Results.BadRequest(new { Message = "We failed to create the bot, invalid options provided." }); }

            if (services!.Runner == null)
                return Results.BadRequest(new { Message = "You have not set any options yet." });

            await services.Runner!.Continue();
            return Results.Ok();
        };

        app.MapPost("/suspend", Suspend()).RequireAuthorization().RequireCors("General-Cors");
        Func<Task<IResult>> Suspend() => async () =>
        {
            if (services == null || services.Runner == null)
                return Results.BadRequest(new { Message = "You have not started any bots yet." });

            await services.Runner!.Suspend();
            return Results.Ok();
        };

        app.MapPost("/stop", Stop()).RequireAuthorization().RequireCors("General-Cors");
        Func<Task<IResult>> Stop() => async () =>
        {
            if (services == null || services.Runner == null)
                return Results.BadRequest(new { Message = "You have not started any bots yet." });

            await services.Runner!.Stop();
            return Results.Ok();
        };

        app.Run();
    }

    private static Services? CreateServices(Options options)
    {
        Services services = new();

        services.PositionRepository = PositionRepositoryFactory.CreateRepository(OptionsMetaData.PositionRepositoryName!);

        services.MessageRepository = MessageRepositoryFactory.CreateRepository(OptionsMetaData.MessageRepositoryName!);

        services.Notifier = NotifierFactory.CreateNotifier(OptionsMetaData.NotifierName!, services.MessageRepository, _logger);

        services.Time = new Time();

        services.Broker = BrokerFactory.CreateBroker(OptionsMetaData.BrokerName!, options.BrokerOptions!, _logger, services.Time);

        services.MessageStore = MessageStoreFactory.CreateMessageStore(OptionsMetaData.MessageStoreName!, options.MessageStoreOptions!, services.MessageRepository, _logger);

        services.RiskManagement = RiskManagementFactory.CreateRiskManager(OptionsMetaData.RiskManagementName!, options.RiskManagementOptions!, services.Broker, services.Time, _logger);

        services.Strategy = StrategyFactory.CreateStrategy(OptionsMetaData.StrategyName!, options.StrategyOptions!, options.IndicatorOptions!, services.RiskManagement, services.Broker, services.Notifier!, services.MessageRepository!, _logger);

        services.Bot = BotFactory.CreateBot(OptionsMetaData.BotName!, services.Broker, options.BotOptions!, services.MessageStore, services.RiskManagement, services.Time, services.Notifier!, _logger);

        services.Runner = RunnerFactory.CreateRunner(OptionsMetaData.RunnerName!, options.RunnerOptions!, services.Bot, services.Broker, services.Strategy, services.Time, services.Notifier!, _logger);

        return services;
    }
}
