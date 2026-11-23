using VectraPKM.Projects.Domain.Entities;

namespace VectraPKM.Projects.Application;

/// <summary>Хранилище личных проектов.</summary>
public interface IProjectStore
{
    /// <summary>Сохраняет новый проект.</summary>
    Task AddAsync(Project project, CancellationToken cancellationToken);

    /// <summary>Возвращает проекты указанного владельца.</summary>
    Task<IReadOnlyList<Project>> ListByOwnerAsync(Guid ownerId, CancellationToken cancellationToken);

    /// <summary>Находит проект по идентификатору и владельцу.</summary>
    Task<Project?> FindByOwnerAsync(Guid id, Guid ownerId, CancellationToken cancellationToken);

    /// <summary>Сохраняет новое название и время изменения проекта указанного владельца.</summary>
    Task<bool> SaveNameAsync(Project project, CancellationToken cancellationToken);
}
