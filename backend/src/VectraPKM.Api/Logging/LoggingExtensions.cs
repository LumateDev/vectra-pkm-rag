using Serilog;

namespace VectraPKM.Api.Logging;

public static class LoggingExtensions
{
    public static Serilog.ILogger CreateBootstrapLogger(string serviceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        return new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", serviceName)
            .WriteTo.Console()
            .CreateBootstrapLogger();
    }

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
