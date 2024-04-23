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

        StaticOptions.FullName = _configuration[ConfigurationKeys.FULL_NAME];
        StaticOptions.PositionRepositoryName = _configuration[ConfigurationKeys.POSITION_REPOSITORY_NAME];
        StaticOptions.MessageRepositoryName = _configuration[ConfigurationKeys.MESSAGE_REPOSITORY_NAME];
        StaticOptions.NotifierName = _configuration[ConfigurationKeys.NOTIFIER_NAME];
        StaticOptions.MessageStoreName = _configuration[ConfigurationKeys.MESSAGE_STORE_NAME];
        StaticOptions.BrokerName = _configuration[ConfigurationKeys.BROKER_NAME];
        StaticOptions.RiskManagementName = _configuration[ConfigurationKeys.RISK_MANAGEMENT_NAME];
        StaticOptions.StrategyName = _configuration[ConfigurationKeys.STRATEGY_NAME];
        StaticOptions.IndicatorOptionsName = _configuration[ConfigurationKeys.INDICATOR_OPTIONS_NAME];
        StaticOptions.BotName = _configuration[ConfigurationKeys.BOT_NAME];
        StaticOptions.RunnerName = _configuration[ConfigurationKeys.RUNNER_NAME];

        _logger.Debug("ASPNETCORE_ENVIRONMENT: {ASPNETCORE_ENVIRONMENT}", _configuration["ASPNETCORE_ENVIRONMENT"]);
        _logger.Debug("ENVIRONMENT: {ENVIRONMENT}", _configuration["ENVIRONMENT"]);
        _logger.Debug("SECRETS_PREFIX: {SECRETS_PREFIX}", _configuration["SECRETS_PREFIX"]);
        _logger.Debug("Serilog:WriteTo:1:Args:serverUrl: {Serilog:WriteTo:1:Args:serverUrl}", _configuration["Serilog:WriteTo:1:Args:serverUrl"]);
        _logger.Debug("PositionRepositoryName: {PositionRepositoryName}", StaticOptions.PositionRepositoryName);
        _logger.Debug("MessageRepositoryName: {MessageRepositoryName}", StaticOptions.MessageRepositoryName);
        _logger.Debug("NotifierName: {NotifierName}", StaticOptions.NotifierName);
        _logger.Debug("MessageStoreName: {MessageStoreName}", StaticOptions.MessageStoreName);
        _logger.Debug("BrokerName: {BrokerName}", StaticOptions.BrokerName);
        _logger.Debug("RiskManagementName: {RiskManagementName}", StaticOptions.RiskManagementName);
        _logger.Debug("StrategyName: {StrategyName}", StaticOptions.StrategyName);
        _logger.Debug("IndicatorOptionsName: {IndicatorOptionsName}", StaticOptions.IndicatorOptionsName);
        _logger.Debug("BotName: {BotName}", StaticOptions.BotName);
        _logger.Debug("RunnerName: {RunnerName}", StaticOptions.RunnerName);

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

        app.MapGet("/options", () => Results.Ok(GetOptions(options))).RequireAuthorization().RequireCors("General-Cors");

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
                    options.MessageStoreOptions = (IMessageStoreOptions)JsonSerializer.Deserialize(JsonSerializer.Serialize(dto.MessageStoreOptions, jsonSerializerOptions), MessageStoreOptionsFactory.GetInstanceType(StaticOptions.MessageStoreName)!, jsonSerializerOptions)!;
                if (dto.BotOptions != null)
                    options.BotOptions = (IBotOptions)JsonSerializer.Deserialize(JsonSerializer.Serialize(dto.BotOptions, jsonSerializerOptions), BotOptionsFactory.GetInstanceType(StaticOptions.BotName)!, jsonSerializerOptions)!;
                if (dto.RunnerOptions != null)
                    options.RunnerOptions = (IRunnerOptions)JsonSerializer.Deserialize(JsonSerializer.Serialize(dto.RunnerOptions, jsonSerializerOptions), RunnerOptionsFactory.GetInstanceType(StaticOptions.RunnerName)!, jsonSerializerOptions)!;
                if (dto.StrategyOptions != null)
                    options.StrategyOptions = (IStrategyOptions)JsonSerializer.Deserialize(JsonSerializer.Serialize(dto.StrategyOptions, jsonSerializerOptions), StrategyOptionsFactory.GetInstanceType(StaticOptions.StrategyName)!, jsonSerializerOptions)!;
                if (dto.IndicatorOptions != null)
                    options.IndicatorOptions = (IIndicatorOptions)JsonSerializer.Deserialize(JsonSerializer.Serialize(dto.IndicatorOptions, jsonSerializerOptions), IndicatorOptionsFactory.GetInstanceType(StaticOptions.IndicatorOptionsName)!, jsonSerializerOptions)!;
                if (dto.RiskManagementOptions != null)
                    options.RiskManagementOptions = (IRiskManagementOptions)JsonSerializer.Deserialize(JsonSerializer.Serialize(dto.RiskManagementOptions, jsonSerializerOptions), RiskManagementOptionsFactory.GetInstanceType(StaticOptions.RiskManagementName)!, jsonSerializerOptions)!;
                if (dto.BrokerOptions != null)
                    options.BrokerOptions = (IBrokerOptions)JsonSerializer.Deserialize(JsonSerializer.Serialize(dto.BrokerOptions, jsonSerializerOptions), BrokerOptionsFactory.GetInstanceType(StaticOptions.BrokerName)!, jsonSerializerOptions)!;

                services = CreateServices(options);

                if (previousStatus == RunnerStatus.RUNNING)
                    Start();
                if (previousStatus == RunnerStatus.SUSPENDED)
                    Suspend();

                return Results.Ok(GetOptions(options));
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

        app.MapGet("/closed-positions", async ([FromQuery] int? limit, [FromQuery] string? startTs, [FromQuery] string? endTs) =>
        {
            IBroker broker = GetServices(options, ref services)?.Broker ?? throw new BadHttpRequestException("Services are not initialized.");

            return Results.Ok(await broker!.GetClosedPositions(start: startTs == null ? null : DateTime.Parse(startTs!).ToUniversalTime(), end: endTs == null ? null : DateTime.Parse(endTs!).ToUniversalTime(), limit: limit));
        }).RequireAuthorization().RequireCors("General-Cors");

        app.MapGet("/open-positions", async () =>
        {
            IBroker broker = GetServices(options, ref services)?.Broker ?? throw new BadHttpRequestException("Services are not initialized.");

            return Results.Ok(await broker!.GetOpenPositions());
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

        app.MapGet("/general-info", () =>
        {
            IBroker broker = GetServices(options, ref services)?.Broker ?? throw new BadHttpRequestException("Services are not initialized.");

            return Results.Ok(new
            {
                fullName = StaticOptions.FullName ?? "FirstName-LastName",
                brokerName = StaticOptions.BrokerName ?? "BrokerName",
            });
        });
    }

    private static object? GetOptions(Options options) =>
        new
        {
            StaticOptions.FullName,
            StaticOptions.BrokerName,
            MessageStoreOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.MessageStoreOptions, MessageStoreOptionsFactory.GetInstanceType(StaticOptions.MessageStoreName)!), MessageStoreOptionsFactory.GetInstanceType(StaticOptions.MessageStoreName)!),
            BotOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.BotOptions, BotOptionsFactory.GetInstanceType(StaticOptions.BotName)!), BotOptionsFactory.GetInstanceType(StaticOptions.BotName)!),
            RunnerOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.RunnerOptions, RunnerOptionsFactory.GetInstanceType(StaticOptions.RunnerName)!), RunnerOptionsFactory.GetInstanceType(StaticOptions.RunnerName)!),
            StrategyOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.StrategyOptions, StrategyOptionsFactory.GetInstanceType(StaticOptions.StrategyName)!), StrategyOptionsFactory.GetInstanceType(StaticOptions.StrategyName)!),
            IndicatorOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.IndicatorOptions, IndicatorOptionsFactory.GetInstanceType(StaticOptions.IndicatorOptionsName)!), IndicatorOptionsFactory.GetInstanceType(StaticOptions.IndicatorOptionsName)!),
            RiskManagementOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.RiskManagementOptions, RiskManagementOptionsFactory.GetInstanceType(StaticOptions.RiskManagementName)!), RiskManagementOptionsFactory.GetInstanceType(StaticOptions.RiskManagementName)!),
            BrokerOptions = JsonSerializer.Deserialize(JsonSerializer.Serialize(options.BrokerOptions, BrokerOptionsFactory.GetInstanceType(StaticOptions.BrokerName)!), BrokerOptionsFactory.GetInstanceType(StaticOptions.BrokerName)!)
        };

    private static Services? GetServices(Options? options, ref Services? services) => services ??= CreateServices(options);

    private static Services? CreateServices(Options? options)
    {
        if (options == null || !IsValid(options))
            return null;

        Services services = new();

        services.PositionRepository = PositionRepositoryFactory.CreateRepository(StaticOptions.PositionRepositoryName!);

        services.MessageRepository = MessageRepositoryFactory.CreateRepository(StaticOptions.MessageRepositoryName!);

        services.Notifier = NotifierFactory.CreateNotifier(StaticOptions.NotifierName!, services.MessageRepository, _logger);

        services.Time = new Time();

        services.Broker = BrokerFactory.CreateBroker(StaticOptions.BrokerName!, options.BrokerOptions!, _logger, services.Time, services.PositionRepository);

        services.MessageStore = MessageStoreFactory.CreateMessageStore(StaticOptions.MessageStoreName!, options.MessageStoreOptions!, services.MessageRepository, _logger);

        services.RiskManagement = RiskManagementFactory.CreateRiskManager(StaticOptions.RiskManagementName!, options.RiskManagementOptions!, services.Broker, services.Time, _logger);

        services.Strategy = StrategyFactory.CreateStrategy(StaticOptions.StrategyName!, options.StrategyOptions!, options.IndicatorOptions!, services.RiskManagement, services.Broker, services.Notifier!, services.MessageRepository!, _logger);

        services.Bot = BotFactory.CreateBot(StaticOptions.BotName!, services.Broker, options.BotOptions!, services.MessageStore, services.RiskManagement, services.Time, services.Notifier!, _logger);

        services.Runner = RunnerFactory.CreateRunner(StaticOptions.RunnerName!, options.RunnerOptions!, services.Bot, services.Broker, services.Strategy, services.Time, services.Notifier!, _logger);

        return services;
    }

    private static bool IsValid(Options? options) => options != null && options.RunnerOptions != null && options.BotOptions != null && options.BrokerOptions != null && options.StrategyOptions != null && options.MessageStoreOptions != null && options.RiskManagementOptions != null && options.IndicatorOptions != null;
}
