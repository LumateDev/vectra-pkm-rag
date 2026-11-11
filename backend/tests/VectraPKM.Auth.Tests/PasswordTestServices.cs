using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VectraPKM.Auth.Infrastructure;
using VectraPKM.Auth.Infrastructure.Identity;

namespace VectraPKM.Auth.Tests;

/// <summary>
/// Подготовка DI для тестов Identity с настоящими сервисами и хранилищем пользователей в памяти.
/// </summary>
internal static class PasswordTestServices
{
    /// <summary>
    /// Создаёт изолированный набор сервисов для одного тестового случая.
    /// EF-хранилище заменяется тестовым; подключение к PostgreSQL не открывается.
    /// </summary>
    internal static ServiceProvider Create()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            // Строка нужна только для регистрации DI. Тесты не используют это подключение к PostgreSQL.
            ["ConnectionStrings:Auth"] = "Host=localhost;Database=unused_password_tests;Username=unused"
        }).Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthInfrastructure(configuration);
        services.AddSingleton<InMemoryUserStore>();
        services.Replace(ServiceDescriptor.Scoped<IUserStore<ApplicationUser>>(provider =>
            provider.GetRequiredService<InMemoryUserStore>()));

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }
}
