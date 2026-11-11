using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Serilog;
using VectraPKM.Api.Logging;
using VectraPKM.Auth.Api;
using VectraPKM.Auth.Api.Contracts;
using VectraPKM.Shared.Api.Contracts;

Log.Logger = LoggingExtensions.CreateBootstrapLogger("VectraPKM.Api");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddVectraLogging("VectraPKM.Api");
    builder.Services.AddAuthModule(builder.Configuration);
    builder.Services.AddControllers();
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressMapClientErrors = true;
        options.InvalidModelStateResponseFactory = context =>
        {
            var messages = context.ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "Некорректное тело запроса."
                    : error.ErrorMessage)
                .Distinct();

            return new BadRequestObjectResult(
                new ApiResult<object>(error: string.Join(" ", messages)));
        };
    });
    builder.Services.AddSwaggerGen(options =>
    {
        options.SupportNonNullableReferenceTypes();
        options.NonNullableReferenceTypesAsRequired();
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
            $"{typeof(RegisterRequest).Assembly.GetName().Name}.xml"));

        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "VectraPKM API",
            Version = "v1"
        });
    });

    var app = builder.Build();
    app.UseSerilogRequestLogging();
    // Неожиданные исключения логируются middleware, но их подробности не попадают в HTTP-ответ.
    app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.Headers.CacheControl = "no-store";
        await context.Response.WriteAsJsonAsync(
            new ApiResult<object>(error: "Произошла непредвиденная ошибка."),
            context.RequestAborted);
    }));

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

