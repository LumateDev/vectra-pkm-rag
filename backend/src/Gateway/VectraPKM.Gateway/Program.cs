using Serilog;
using VectraPKM.Logging;

Log.Logger = LoggingExtensions.CreateBootstrapLogger("Gateway");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddVectraLogging("Gateway");

    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    var app = builder.Build();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/api/identity/swagger/v1/swagger.json", "Identity v1");
            options.DocumentTitle = "VectraPKM API";
        });
    }

    app.MapReverseProxy();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway terminated unexpectedly");
    Environment.ExitCode = 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
