using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VectraPKM.Projects.Api.Controllers;
using VectraPKM.Projects.Application;
using VectraPKM.Projects.Infrastructure;

namespace VectraPKM.Projects.Api;

/// <summary>Точка подключения модуля Projects к приложению.</summary>
public static class ProjectsModule
{
    /// <summary>Регистрирует сценарии, хранилище и контроллеры модуля.</summary>
    public static IServiceCollection AddProjectsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddProjectsInfrastructure(configuration);
        services.AddScoped<ProjectService>();
        services.AddControllers().AddApplicationPart(typeof(ProjectsController).Assembly);
        return services;
    }
}
