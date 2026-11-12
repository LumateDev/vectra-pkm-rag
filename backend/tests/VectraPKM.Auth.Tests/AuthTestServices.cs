using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VectraPKM.Auth.Infrastructure;
using VectraPKM.Auth.Infrastructure.Identity;
using VectraPKM.Auth.Infrastructure.Persistence;

namespace VectraPKM.Auth.Tests;

/// <summary>
/// Подготовка DI для тестов Identity с настоящим EF-хранилищем и провайдером InMemory.
/// </summary>
internal static class AuthTestServices
{
    internal const string JwtKey = "VectraPKM-Tests-Only-Symmetric-Signing-Key-Not-For-Production-2026!!";

    /// <summary>
    /// Создаёт изолированный набор сервисов для одного тестового случая.
    /// Каждый набор сервисов получает отдельную БД InMemory; подключение к PostgreSQL не открывается.
    /// </summary>
    internal static ServiceProvider Create(int accessTokenLifetimeMinutes = 15)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            // Строка нужна только для регистрации DI. Тесты не используют это подключение к PostgreSQL.
            ["ConnectionStrings:Auth"] = "Host=localhost;Database=unused_password_tests;Username=unused",
            ["Jwt:Issuer"] = "VectraPKM.Tests",
            ["Jwt:Audience"] = "VectraPKM.Tests.Api",
            ["Jwt:Key"] = JwtKey,
            ["Jwt:AccessTokenLifetimeMinutes"] = accessTokenLifetimeMinutes.ToString()
        }).Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRouting();
        services.AddAuthInfrastructure(configuration);
        // Удаляем также настройку UseNpgsql, чтобы у контекста остался ровно один провайдер.
        services.RemoveAll<AuthDbContext>();
        services.RemoveAll<DbContextOptions<AuthDbContext>>();
        services.RemoveAll<IDbContextOptionsConfiguration<AuthDbContext>>();
        var databaseName = $"AuthTests-{Guid.NewGuid():N}";
        services.AddDbContext<AuthDbContext>(options => options.UseInMemoryDatabase(databaseName));

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    /// <summary>Читает сохранённых пользователей через новый контекст без отслеживания изменений.</summary>
    internal static async Task<List<ApplicationUser>> ReadUsersAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        return await context.Users.AsNoTracking().ToListAsync();
    }
}
