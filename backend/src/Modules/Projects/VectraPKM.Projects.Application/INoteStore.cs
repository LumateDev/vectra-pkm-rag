using VectraPKM.Projects.Domain.Entities;

namespace VectraPKM.Projects.Application;

/// <summary>Хранилище заметок проекта.</summary>
public interface INoteStore
{
    /// <summary>Сохраняет новую заметку.</summary>
    Task AddAsync(Note note, CancellationToken cancellationToken);

    /// <summary>Возвращает список заметок указанного проекта.</summary>
    Task<IReadOnlyList<NoteSummary>> ListAsync(Guid projectId, CancellationToken cancellationToken);
}
