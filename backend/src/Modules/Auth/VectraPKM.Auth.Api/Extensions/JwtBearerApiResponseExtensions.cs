using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using VectraPKM.Shared.Api.Contracts;

namespace VectraPKM.Auth.Api.Extensions;

/// <summary>Настройка HTTP-ответов JWT Bearer в формате ApiResult.</summary>
internal static class JwtBearerApiResponseExtensions
{
    /// <summary>Настраивает ответы 401 и 403 для схемы Bearer.</summary>
    internal static IServiceCollection AddJwtBearerApiResponses(this IServiceCollection services)
    {
        services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    if (context.Response.HasStarted) return;

                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.Headers.WWWAuthenticate = "Bearer";
                    await context.Response.WriteAsJsonAsync(
                        new ApiResult<object>(error: "Требуется авторизация."), context.HttpContext.RequestAborted);
                },
                OnForbidden = async context =>
                {
                    if (context.Response.HasStarted) return;

                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(
                        new ApiResult<object>(error: "Недостаточно прав для выполнения операции."),
                        context.HttpContext.RequestAborted);
                }
            };
        });

        return services;
    }
}
