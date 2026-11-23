using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VectraPKM.Projects.Application;
using VectraPKM.Projects.Infrastructure.Persistence;

namespace VectraPKM.Projects.Infrastructure;

/// <summary>Регистрация контекста данных личных проектов.</summary>
public static class DependencyInjection
{
    /// <summary>Регистрирует ProjectsDbContext в той же базе PostgreSQL, что и Auth.</summary>
    /// <param name="services">Коллекция сервисов приложения.</param>
    /// <param name="configuration">Конфигурация со строкой подключения ConnectionStrings:Auth.</param>
    /// <returns>Коллекция сервисов приложения.</returns>
    public static IServiceCollection AddProjectsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Auth");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Не задана строка подключения ConnectionStrings:Auth.");

        services.AddDbContext<ProjectsDbContext>(options =>
            options.UseNpgsql(connectionString,
                postgres => postgres.MigrationsHistoryTable("__ProjectsMigrationsHistory")));
        services.AddScoped<IProjectStore, ProjectStore>();
        services.AddScoped<INoteStore, NoteStore>();
        return services;
    }
}
