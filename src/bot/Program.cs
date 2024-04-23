using Abstractions.src.Models;
using Abstractions.src.Bots;
using Abstractions.src.Brokers;
using Abstractions.src.Indicators;
using Abstractions.src.RiskManagement;
using Abstractions.src.Runners;
using Abstractions.src.Strategies;
using Bots.src;
using Brokers.src;
using Data.src;
using Indicators.src;
using MessageStores.src;
using Notifiers.src;
using RiskManagement.src;
using Runners.src;
using Strategies.src;
using Utilities.src;
using ILogger = Serilog.ILogger;
using Serilog.Settings.Configuration;
using Serilog;
using bot.src.Configuration.Providers.DockerSecrets;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using bot.src.Authentication.ApiKey;
using Microsoft.AspNetCore.Mvc;
using Abstractions.src.MessageStores;
using bot.src.Dtos;

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

        JsonSerializerOptions jsonSerializerOptions = new()
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString | System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals | System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
            WriteIndented = true,
        };
        builder.Services.ConfigureHttpJsonOptions(o =>
        {
            o.SerializerOptions.AllowTrailingCommas = jsonSerializerOptions.AllowTrailingCommas;
            o.SerializerOptions.PropertyNameCaseInsensitive = jsonSerializerOptions.PropertyNameCaseInsensitive;
            o.SerializerOptions.NumberHandling = jsonSerializerOptions.NumberHandling;
            o.SerializerOptions.WriteIndented = jsonSerializerOptions.WriteIndented;
        });

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

        ConfigureEndPoints(app, options, services, jsonSerializerOptions);

        app.Run();
    }

    private static void ConfigureEndPoints(WebApplication app, Options options, Services? services, JsonSerializerOptions jsonSerializerOptions)
    {
        // ------------------------------------------------------------------------------------------------ Options

        app.MapGet("/options", () => Results.Ok(
            new
            {
                MessageStoreOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.MessageStoreOptions, MessageStoreOptionsFactory.GetInstanceType(OptionsMetaData.MessageStoreName)!, jsonSerializerOptions), MessageStoreOptionsFactory.GetInstanceType(OptionsMetaData.MessageStoreName)!, jsonSerializerOptions),
                BotOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.BotOptions, BotOptionsFactory.GetInstanceType(OptionsMetaData.BotName)!, jsonSerializerOptions), BotOptionsFactory.GetInstanceType(OptionsMetaData.BotName)!, jsonSerializerOptions),
                RunnerOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.RunnerOptions, RunnerOptionsFactory.GetInstanceType(OptionsMetaData.RunnerName)!, jsonSerializerOptions), RunnerOptionsFactory.GetInstanceType(OptionsMetaData.RunnerName)!, jsonSerializerOptions),
                StrategyOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.StrategyOptions, StrategyOptionsFactory.GetInstanceType(OptionsMetaData.StrategyName)!, jsonSerializerOptions), StrategyOptionsFactory.GetInstanceType(OptionsMetaData.StrategyName)!, jsonSerializerOptions),
                IndicatorOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.IndicatorOptions, IndicatorOptionsFactory.GetInstanceType(OptionsMetaData.IndicatorOptionsName)!, jsonSerializerOptions), IndicatorOptionsFactory.GetInstanceType(OptionsMetaData.IndicatorOptionsName)!, jsonSerializerOptions),
                RiskManagementOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.RiskManagementOptions, RiskManagementOptionsFactory.GetInstanceType(OptionsMetaData.RiskManagementName)!, jsonSerializerOptions), RiskManagementOptionsFactory.GetInstanceType(OptionsMetaData.RiskManagementName)!, jsonSerializerOptions),
                BrokerOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.BrokerOptions, BrokerOptionsFactory.GetInstanceType(OptionsMetaData.BrokerName)!, jsonSerializerOptions), BrokerOptionsFactory.GetInstanceType(OptionsMetaData.BrokerName)!, jsonSerializerOptions)
            }
        )).RequireAuthorization().RequireCors("General-Cors");

        app.MapPatch("/options", async (OptionsDto dto) =>
        {
            try
            {
                DateTime now = DateTime.UtcNow;
                while (now.Second < 7 || now.Second > 53)
                {
                    await Task.Delay(1000);
                    now = now.AddSeconds(1);
                }

                RunnerStatus previousStatus = services?.Runner!.Status ?? RunnerStatus.STOPPED;
                Stop();

                if (dto.MessageStoreOptions != null)
                    options.MessageStoreOptions = (IMessageStoreOptions)JsonSerializer.Deserialize(JsonSerializer.Serialize(dto.MessageStoreOptions, jsonSerializerOptions), MessageStoreOptionsFactory.GetInstanceType(OptionsMetaData.MessageStoreName)!, jsonSerializerOptions)!;
                if (dto.BotOptions != null)
                    options.BotOptions = (IBotOptions)JsonSerializer.Deserialize(JsonSerializer.Serialize(dto.BotOptions, jsonSerializerOptions), BotOptionsFactory.GetInstanceType(OptionsMetaData.BotName)!, jsonSerializerOptions)!;
                if (dto.RunnerOptions != null)
                    options.RunnerOptions = (IRunnerOptions)JsonSerializer.Deserialize(JsonSerializer.Serialize(dto.RunnerOptions, jsonSerializerOptions), RunnerOptionsFactory.GetInstanceType(OptionsMetaData.RunnerName)!, jsonSerializerOptions)!;
                if (dto.StrategyOptions != null)
                    options.StrategyOptions = (IStrategyOptions)JsonSerializer.Deserialize(JsonSerializer.Serialize(dto.StrategyOptions, jsonSerializerOptions), StrategyOptionsFactory.GetInstanceType(OptionsMetaData.StrategyName)!, jsonSerializerOptions)!;
                if (dto.IndicatorOptions != null)
                    options.IndicatorOptions = (IIndicatorOptions)JsonSerializer.Deserialize(JsonSerializer.Serialize(dto.IndicatorOptions, jsonSerializerOptions), IndicatorOptionsFactory.GetInstanceType(OptionsMetaData.IndicatorOptionsName)!, jsonSerializerOptions)!;
                if (dto.RiskManagementOptions != null)
                    options.RiskManagementOptions = (IRiskManagementOptions)JsonSerializer.Deserialize(JsonSerializer.Serialize(dto.RiskManagementOptions, jsonSerializerOptions), RiskManagementOptionsFactory.GetInstanceType(OptionsMetaData.RiskManagementName)!, jsonSerializerOptions)!;
                if (dto.BrokerOptions != null)
                    options.BrokerOptions = (IBrokerOptions)JsonSerializer.Deserialize(JsonSerializer.Serialize(dto.BrokerOptions, jsonSerializerOptions), BrokerOptionsFactory.GetInstanceType(OptionsMetaData.BrokerName)!, jsonSerializerOptions)!;

                services = CreateServices(options);

                if (previousStatus == RunnerStatus.RUNNING)
                    Start();
                if (previousStatus == RunnerStatus.SUSPENDED)
                    Suspend();

                return Results.Ok(
                    new
                    {
                        MessageStoreOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.MessageStoreOptions, MessageStoreOptionsFactory.GetInstanceType(OptionsMetaData.MessageStoreName)!), MessageStoreOptionsFactory.GetInstanceType(OptionsMetaData.MessageStoreName)!),
                        BotOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.BotOptions, BotOptionsFactory.GetInstanceType(OptionsMetaData.BotName)!), BotOptionsFactory.GetInstanceType(OptionsMetaData.BotName)!),
                        RunnerOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.RunnerOptions, RunnerOptionsFactory.GetInstanceType(OptionsMetaData.RunnerName)!), RunnerOptionsFactory.GetInstanceType(OptionsMetaData.RunnerName)!),
                        StrategyOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.StrategyOptions, StrategyOptionsFactory.GetInstanceType(OptionsMetaData.StrategyName)!), StrategyOptionsFactory.GetInstanceType(OptionsMetaData.StrategyName)!),
                        IndicatorOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.IndicatorOptions, IndicatorOptionsFactory.GetInstanceType(OptionsMetaData.IndicatorOptionsName)!), IndicatorOptionsFactory.GetInstanceType(OptionsMetaData.IndicatorOptionsName)!),
                        RiskManagementOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.RiskManagementOptions, RiskManagementOptionsFactory.GetInstanceType(OptionsMetaData.RiskManagementName)!), RiskManagementOptionsFactory.GetInstanceType(OptionsMetaData.RiskManagementName)!),
                        BrokerOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.BrokerOptions, BrokerOptionsFactory.GetInstanceType(OptionsMetaData.BrokerName)!), BrokerOptionsFactory.GetInstanceType(OptionsMetaData.BrokerName)!)
                    }
                );
            }
            catch (Exception) { return Results.Problem("Server failed to process your request."); }
        }).RequireAuthorization().RequireCors("General-Cors");

        // ------------------------------------------------------------------------------------------------ Status

        app.MapGet("/status", () =>
        {
            return Results.Ok(new { Status = GetServices(options, ref services)?.Runner?.Status.ToString() ?? throw new BadHttpRequestException("Services are not initialized.") });
        }).RequireAuthorization().RequireCors("General-Cors");

        app.MapPost("/start", Start()).RequireAuthorization().RequireCors("General-Cors");
        Func<IResult> Start() => () =>
        {
            IRunner runner = GetServices(options, ref services)?.Runner ?? throw new BadHttpRequestException("Services are not initialized.");
            runner!.Continue();
            return Results.Ok();
        };

        app.MapPost("/suspend", Suspend()).RequireAuthorization().RequireCors("General-Cors");
        Func<Task<IResult>> Suspend() => async () =>
        {
            IRunner runner = GetServices(options, ref services)?.Runner ?? throw new BadHttpRequestException("Services are not initialized.");
            await runner.Suspend();
            return Results.NoContent();
        };

        app.MapPost("/stop", Stop()).RequireAuthorization().RequireCors("General-Cors");
        Func<Task<IResult>> Stop() => async () =>
        {
            IRunner runner = GetServices(options, ref services)?.Runner ?? throw new BadHttpRequestException("Services are not initialized.");
            await runner.Stop();
            return Results.NoContent();
        };

        // ------------------------------------------------------------------------------------------------ Position

        app.MapGet("/closed-positions", async ([FromQuery] string? startTs, [FromQuery] string? endTs) =>
        {
            IBroker broker = GetServices(options, ref services)?.Broker ?? throw new BadHttpRequestException("Services are not initialized.");

            return Results.Ok(startTs != null && endTs != null
                ? await broker!.GetClosedPositions(start: DateTime.Parse(startTs!).ToUniversalTime(), end: DateTime.Parse(endTs!).ToUniversalTime())
                : (startTs != null
                    ? await broker!.GetClosedPositions(start: DateTime.Parse(startTs!).ToUniversalTime())
                    : (endTs != null
                        ? await broker!.GetClosedPositions(end: DateTime.Parse(endTs!).ToUniversalTime())
                        : await broker!.GetClosedPositions()
                    )
                )
            );
        }).RequireAuthorization().RequireCors("General-Cors");

        app.MapGet("/assets", async () =>
        {
            IBroker broker = GetServices(options, ref services)?.Broker ?? throw new BadHttpRequestException("Services are not initialized.");

            return Results.Ok(await broker!.GetAssets());
        });

        app.MapGet("/pnl", async () =>
        {
            IBroker broker = GetServices(options, ref services)?.Broker ?? throw new BadHttpRequestException("Services are not initialized.");

            return Results.Ok(await broker!.GetPnlFundFlow());
        });

        app.MapGet("/general-info", async () =>
        {
            IBroker broker = GetServices(options, ref services)?.Broker ?? throw new BadHttpRequestException("Services are not initialized.");

            return Results.Ok(new
            {
                fullName = app.Configuration[ConfigurationKeys.FULL_NAME] ?? "FirstName-LastName",
                brokerName = app.Configuration[ConfigurationKeys.BROKER_NAME] ?? "BrokerName",
            });
        });
    }

    private static Services? GetServices(Options? options, ref Services? services) => services ??= CreateServices(options);

    private static Services? CreateServices(Options? options)
    {
        if (options == null || !IsValid(options))
            return null;

        Services services = new();

        services.PositionRepository = PositionRepositoryFactory.CreateRepository(OptionsMetaData.PositionRepositoryName!);

        services.MessageRepository = MessageRepositoryFactory.CreateRepository(OptionsMetaData.MessageRepositoryName!);

        services.Notifier = NotifierFactory.CreateNotifier(OptionsMetaData.NotifierName!, services.MessageRepository, _logger);

        services.Time = new Time();

        services.Broker = BrokerFactory.CreateBroker(OptionsMetaData.BrokerName!, options.BrokerOptions!, _logger, services.Time, services.PositionRepository);

        services.MessageStore = MessageStoreFactory.CreateMessageStore(OptionsMetaData.MessageStoreName!, options.MessageStoreOptions!, services.MessageRepository, _logger);

        services.RiskManagement = RiskManagementFactory.CreateRiskManager(OptionsMetaData.RiskManagementName!, options.RiskManagementOptions!, services.Broker, services.Time, _logger);

        services.Strategy = StrategyFactory.CreateStrategy(OptionsMetaData.StrategyName!, options.StrategyOptions!, options.IndicatorOptions!, services.RiskManagement, services.Broker, services.Notifier!, services.MessageRepository!, _logger);

        services.Bot = BotFactory.CreateBot(OptionsMetaData.BotName!, services.Broker, options.BotOptions!, services.MessageStore, services.RiskManagement, services.Time, services.Notifier!, _logger);

        services.Runner = RunnerFactory.CreateRunner(OptionsMetaData.RunnerName!, options.RunnerOptions!, services.Bot, services.Broker, services.Strategy, services.Time, services.Notifier!, _logger);

        return services;
    }

    private static bool IsValid(Options? options) => options != null && options.RunnerOptions != null && options.BotOptions != null && options.BrokerOptions != null && options.StrategyOptions != null && options.MessageStoreOptions != null && options.RiskManagementOptions != null && options.IndicatorOptions != null;
}
