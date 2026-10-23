using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VectraPKM.Auth.Infrastructure.Identity;
using VectraPKM.Auth.Infrastructure.Persistence;

namespace VectraPKM.Auth.Infrastructure;

/// <summary>
/// Регистрация инфраструктурных зависимостей модуля Auth.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Регистрирует контекст Auth с провайдером PostgreSQL и сервисы ASP.NET Core Identity.
    /// Подключение к БД и применение миграций не выполняются.
    /// </summary>
    /// <param name="services">Коллекция сервисов приложения.</param>
    /// <param name="configuration">Конфигурация со строкой подключения ConnectionStrings:Auth.</param>
    /// <returns>Коллекция сервисов для дальнейшей регистрации зависимостей.</returns>
    public static IServiceCollection AddAuthInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Auth");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Не задана строка подключения ConnectionStrings:Auth. " +
                "Укажите её в конфигурации или переменной окружения ConnectionStrings__Auth.");
        }

        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;

            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;

            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddPasswordValidator<MaximumPasswordLengthValidator>();

        return services;
    }
}
