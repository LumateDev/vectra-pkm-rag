using VectraPKM.Projects.Domain.Entities;

namespace VectraPKM.Projects.Application;

/// <summary>Хранилище заметок проекта.</summary>
public interface INoteStore
{
    /// <summary>Сохраняет новую заметку.</summary>
    Task AddAsync(Note note, CancellationToken cancellationToken);

    /// <summary>Возвращает список заметок указанного проекта.</summary>
    Task<IReadOnlyList<NoteSummary>> ListAsync(Guid projectId, CancellationToken cancellationToken);

    /// <summary>Находит заметку по идентификатору внутри указанного проекта.</summary>
    Task<Note?> FindAsync(Guid projectId, Guid noteId, CancellationToken cancellationToken);

    /// <summary>Сохраняет заголовок, текст и дату изменения заметки внутри её проекта.</summary>
    Task<bool> SaveContentAsync(Note note, CancellationToken cancellationToken);
}
