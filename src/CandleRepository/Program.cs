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

        var app = builder.Build();

        app.UseRateLimiter();

        app.UseCors("General-Cors");

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

    private static void ConfigureEndPoints(WebApplication app, OptionsMetaData optionsMetaData, Options options, Services? services)
    {
        app.MapGet("/", () => "Hello World!");
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
}