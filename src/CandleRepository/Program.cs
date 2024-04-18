using ILogger = Serilog.ILogger;
using Serilog.Settings.Configuration;
using Serilog;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using CandleRepository.src.Configuration.Providers.DockerSecrets;
using bot;
using Runners.src;
using Abstractions.src.Models;
using Repositories.src;
using Notifiers.src;
using Util.src;
using Brokers.src;
using System.Text.Json.Nodes;
using System.Text.Json;
using Abstractions.src.Notifiers;
using Abstractions.src.Brokers;
using Abstractions.src.RiskManagements;
using Abstractions.src.Bots;
using Bots.src;
using Abstractions.src.Runners;
using Abstractions.src.Strategies;
using Abstractions.src.Indicators;
using Strategies.src;
using Indicators.src;
using RiskManagement.src;
using bot.src.Authentication.ApiKey;

namespace CandleRepository;

public class Program
{
    public const string ENV_PREFIX = "CANDLE_REPOSITORY_";
    public static string RootPath { get; set; } = "";

    private static ConfigurationManager _configuration = null!;
    private static ILogger _logger = null!;

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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

        OptionsMetaData optionsMetaData = new(
            candleRepositoryName: _configuration[ConfigurationKeys.CANDLE_REPOSITORY_NAME],
            positionRepositoryName: _configuration[ConfigurationKeys.POSITION_REPOSITORY_NAME],
            messageRepositoryName: _configuration[ConfigurationKeys.MESSAGE_REPOSITORY_NAME],
            notifierName: _configuration[ConfigurationKeys.NOTIFIER_NAME],
            messageStoreName: _configuration[ConfigurationKeys.MESSAGE_STORE_NAME],
            brokerName: _configuration[ConfigurationKeys.BROKER_NAME],
            riskManagementName: _configuration[ConfigurationKeys.RISK_MANAGEMENT_NAME],
            strategyName: _configuration[ConfigurationKeys.STRATEGY_NAME],
            indicatorOptionsName: _configuration[ConfigurationKeys.INDICATOR_OPTIONS_NAME],
            botName: _configuration[ConfigurationKeys.BOT_NAME],
            runnerName: _configuration[ConfigurationKeys.RUNNER_NAME]
        );

        _logger.Debug("ASPNETCORE_ENVIRONMENT: {ASPNETCORE_ENVIRONMENT}", _configuration["ASPNETCORE_ENVIRONMENT"]);
        _logger.Debug("ENVIRONMENT: {ENVIRONMENT}", _configuration["ENVIRONMENT"]);
        _logger.Debug("SECRETS_PREFIX: {SECRETS_PREFIX}", _configuration["SECRETS_PREFIX"]);
        _logger.Debug("Serilog:WriteTo:1:Args:serverUrl: {Serilog:WriteTo:1:Args:serverUrl}", _configuration["Serilog:WriteTo:1:Args:serverUrl"]);
        _logger.Debug("optionsMetaData: {@optionsMetaData}", optionsMetaData);

        Options? options = new();
        Services? services = null;

        ConfigureEndPoints(app, optionsMetaData, options, services);

        app.Run();
    }

    private static Services? CreateServices(Options options, OptionsMetaData optionsMetaData)
    {
        Services services = new();

        services.CandlesRepository = CandleRepositoryFactory.CreateRepository(optionsMetaData.CandleRepositoryName!);

        services.MessageRepository = MessageRepositoryFactory.CreateRepository(optionsMetaData.MessageRepositoryName!);

        services.Notifier = NotifierFactory.CreateNotifier(optionsMetaData.NotifierName!, services.MessageRepository, _logger, options.NotifierOptions!);

        services.Time = new Time();

        services.Broker = BrokerFactory.CreateBroker(optionsMetaData.BrokerName!, options.BrokerOptions!, _logger, services.Time, services.CandlesRepository, null);

        services.Runner = RunnerFactory.CreateRunner(optionsMetaData.RunnerName!, options.RunnerOptions!, services.CandlesRepository!, null, services.Broker, null, services.Time, services.Notifier!, _logger);

        return services;
    }

    private static void ConfigureEndPoints(WebApplication app, OptionsMetaData optionsMetaData, Options options, Services? services)
    {
        // ------------------------------------------------------------------------------------------------ Options

        app.MapGet("/options", () => Results.Ok(
            new
            {
                BotOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.BotOptions, BotOptionsFactory.GetInstanceType(optionsMetaData.BotName)!), BotOptionsFactory.GetInstanceType(optionsMetaData.BotName)!),
                RunnerOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.RunnerOptions, RunnerOptionsFactory.GetInstanceType(optionsMetaData.RunnerName)!), RunnerOptionsFactory.GetInstanceType(optionsMetaData.RunnerName)!),
                StrategyOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.StrategyOptions, StrategyOptionsFactory.GetInstanceType(optionsMetaData.StrategyName)!), StrategyOptionsFactory.GetInstanceType(optionsMetaData.StrategyName)!),
                IndicatorOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.IndicatorOptions, IndicatorOptionsFactory.GetInstanceType(optionsMetaData.IndicatorOptionsName)!), IndicatorOptionsFactory.GetInstanceType(optionsMetaData.IndicatorOptionsName)!),
                RiskManagementOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.RiskManagementOptions, RiskManagementOptionsFactory.GetInstanceType(optionsMetaData.RiskManagementName)!), RiskManagementOptionsFactory.GetInstanceType(optionsMetaData.RiskManagementName)!),
                BrokerOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.BrokerOptions, BrokerOptionsFactory.GetInstanceType(optionsMetaData.BrokerName)!), BrokerOptionsFactory.GetInstanceType(optionsMetaData.BrokerName)!)
            }
        )).RequireAuthorization().RequireCors("General-Cors");

        app.MapPatch("/runner-options", (JsonNode opt) =>
        {
            try
            {
                string json = JsonSerializer.Serialize(opt);
                options.RunnerOptions = (IRunnerOptions)opt.Deserialize(RunnerOptionsFactory.GetInstanceType(optionsMetaData.RunnerName)!, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;

                return Results.Ok(options.RunnerOptions);
            }
            catch (Exception) { return Results.Problem("Server failed to process your request."); }
        }).RequireAuthorization().RequireCors("General-Cors");

        app.MapPatch("/notifier-options", (JsonNode opt) =>
        {
            try
            {
                string json = JsonSerializer.Serialize(opt);
                options.NotifierOptions = (INotifierOptions)opt.Deserialize(NotifierOptionsFactory.GetInstanceType(optionsMetaData.NotifierName)!, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;

                return Results.Ok(options.NotifierOptions);
            }
            catch (Exception) { return Results.Problem("Server failed to process your request."); }
        }).RequireAuthorization().RequireCors("General-Cors");

        app.MapPatch("/broker-options", (JsonNode opt) =>
        {
            try
            {
                IBrokerOptions? oldOptionsBrokerOptions = options.BrokerOptions;

                string json = JsonSerializer.Serialize(opt);
                IBrokerOptions input = (IBrokerOptions)opt.Deserialize(BrokerOptionsFactory.GetInstanceType(optionsMetaData.BrokerName)!, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;

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
                    services = CreateServices(options!, optionsMetaData);
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
        Func<IResult> Start() => () =>
        {
            if (services == null)
                try { services = CreateServices(options!, optionsMetaData); }
                catch (Exception) { return Results.BadRequest(new { Message = "We failed to create the bot, invalid options provided." }); }

            if (services!.Runner == null)
                return Results.BadRequest(new { Message = "You have not set any options yet." });

            _ = services.Runner!.Continue();
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
    }
}