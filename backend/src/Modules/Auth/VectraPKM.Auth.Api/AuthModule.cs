using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VectraPKM.Auth.Api.Controllers;
using VectraPKM.Auth.Api.Extensions;
using VectraPKM.Auth.Application;
using VectraPKM.Auth.Infrastructure;

namespace VectraPKM.Auth.Api;

/// <summary>
/// Точка подключения модуля Auth к приложению.
/// </summary>
public static class AuthModule
{
    /// <summary>
    /// Регистрирует зависимости модуля Auth.
    /// </summary>
    /// <param name="services">Коллекция сервисов приложения.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <returns>Коллекция сервисов для дальнейшей регистрации зависимостей.</returns>
    public static IServiceCollection AddAuthModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthInfrastructure(configuration);
        services.AddScoped<IAuthService, AuthService>();
        services.AddControllers().AddApplicationPart(typeof(AuthController).Assembly);
        services.AddJwtBearerApiResponses();

        return services;
    }
}
