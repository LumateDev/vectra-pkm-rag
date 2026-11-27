using Serilog;

namespace VectraPKM.Api.Logging;

/// <summary>Настраивает логирование хоста API.</summary>
public static class LoggingExtensions
{
    /// <summary>Создаёт логгер для событий до запуска хоста.</summary>
    public static Serilog.ILogger CreateBootstrapLogger(string serviceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        return new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", serviceName)
            .WriteTo.Console()
            .CreateBootstrapLogger();
    }

    /// <summary>Подключает Serilog с настройками окружения.</summary>
    public static WebApplicationBuilder AddVectraLogging(
        this WebApplicationBuilder builder, string serviceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        builder.Services.AddSerilog((services, configuration) => configuration
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", serviceName)
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName));

        return builder;
    }
}
