using VectraPKM.Projects.Domain.Entities;

namespace VectraPKM.Projects.Application;

/// <summary>Хранилище личных проектов.</summary>
public interface IProjectStore
{
    /// <summary>Сохраняет новый проект.</summary>
    Task AddAsync(Project project, CancellationToken cancellationToken);

    /// <summary>Возвращает проекты указанного владельца.</summary>
    Task<IReadOnlyList<Project>> ListByOwnerAsync(Guid ownerId, CancellationToken cancellationToken);
}
