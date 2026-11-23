using VectraPKM.Projects.Domain.Entities;

namespace VectraPKM.Projects.Application;

/// <summary>Сценарии работы с личными проектами.</summary>
public sealed class ProjectService(IProjectStore store)
{
    /// <summary>Создаёт проект для указанного владельца.</summary>
    public async Task<Project> CreateAsync(Guid ownerId, string name, CancellationToken cancellationToken)
    {
        var project = new Project(ownerId, name, DateTimeOffset.UtcNow);
        await store.AddAsync(project, cancellationToken);
        return project;
    }

    /// <summary>Возвращает проекты указанного владельца.</summary>
    public Task<IReadOnlyList<Project>> ListAsync(Guid ownerId, CancellationToken cancellationToken)
        => store.ListByOwnerAsync(ownerId, cancellationToken);
}
