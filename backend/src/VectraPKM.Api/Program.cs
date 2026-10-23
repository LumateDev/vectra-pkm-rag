using Microsoft.OpenApi;
using Serilog;
using VectraPKM.Api.Logging;
using VectraPKM.Auth.Api;

Log.Logger = LoggingExtensions.CreateBootstrapLogger("VectraPKM.Api");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddVectraLogging("VectraPKM.Api");
    builder.Services.AddAuthModule(builder.Configuration);
    builder.Services.AddControllers();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "VectraPKM API",
            Version = "v1"
        });
    });

    var app = builder.Build();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("v1/swagger.json", "VectraPKM API v1");
            options.DocumentTitle = "VectraPKM API";
        });
    }

    app.MapControllers();
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "VectraPKM API terminated unexpectedly");
    Environment.ExitCode = 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

