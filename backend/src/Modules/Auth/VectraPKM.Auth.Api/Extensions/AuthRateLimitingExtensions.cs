using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VectraPKM.Auth.Api.Configuration;
using VectraPKM.Shared.Api.Contracts;

namespace VectraPKM.Auth.Api.Extensions;

/// <summary>Регистрирует отдельные ограничения auth-endpoint по IP без очереди запросов.</summary>
internal static class AuthRateLimitingExtensions
{
    internal const string LoginPolicy = "auth-login";
    internal const string RegisterPolicy = "auth-register";
    internal const string RefreshPolicy = "auth-refresh";
    internal const string LogoutPolicy = "auth-logout";

    internal static IServiceCollection AddAuthRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(AuthRateLimitOptions.SectionName).Get<AuthRateLimitOptions>()
            ?? new AuthRateLimitOptions();
        settings.Validate();

        services.AddRateLimiter(options =>
        {
            AddPolicy(options, LoginPolicy, settings.Login);
            AddPolicy(options, RegisterPolicy, settings.Register);
            AddPolicy(options, RefreshPolicy, settings.Refresh);
            AddPolicy(options, LogoutPolicy, settings.Logout);

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                var response = context.HttpContext.Response;
                response.StatusCode = StatusCodes.Status429TooManyRequests;
                response.Headers.CacheControl = "no-store";
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    response.Headers.RetryAfter = Math.Max(1, Math.Ceiling(retryAfter.TotalSeconds))
                        .ToString(CultureInfo.InvariantCulture);
                }
                await response.WriteAsJsonAsync(
                    new ApiResult<object>(error: "Слишком много запросов. Повторите попытку позже."), cancellationToken);
            };
        });
        return services;
    }

    private static void AddPolicy(RateLimiterOptions options, string name, EndpointRateLimitOptions settings)
    {
        options.AddPolicy(name, context => RateLimitPartition.GetFixedWindowLimiter(
            // Заголовки X-Forwarded-For не читаем: доверенные прокси настраиваются отдельно в хосте.
            partitionKey: context.Connection.RemoteIpAddress?.MapToIPv6().ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = settings.PermitLimit,
                Window = TimeSpan.FromSeconds(settings.WindowSeconds),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    }
}
