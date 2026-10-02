using Microsoft.OpenApi;
using Serilog;
using VectraPKM.Logging;

Log.Logger = LoggingExtensions.CreateBootstrapLogger("Identity");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddVectraLogging("Identity");

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "VectraPKM Identity API",
            Version = "v1"
        });
    });

    var app = builder.Build();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger(options =>
        {
            options.PreSerializeFilters.Add((document, request) =>
            {
                var prefix = request.Headers["X-Forwarded-Prefix"].ToString();
                document.Servers = [new OpenApiServer
                {
                    Url = string.IsNullOrEmpty(prefix) ? "/" : prefix
                }];
            });
        });
    }

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Identity terminated unexpectedly");
    Environment.ExitCode = 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
