using VectraPKM.Projects.Domain.Entities;

namespace VectraPKM.Projects.Application;

/// <summary>Работа с заметками в личных проектах пользователя.</summary>
public sealed class NoteService(IProjectStore projects, INoteStore notes)
{
    /// <summary>Создаёт заметку, если проект принадлежит указанному владельцу.</summary>
    public async Task<Note?> CreateAsync(
        Guid projectId,
        Guid ownerId,
        string title,
        string markdown,
        CancellationToken cancellationToken)
    {
        var project = await projects.FindByOwnerAsync(projectId, ownerId, cancellationToken);
        if (project is null)
            return null;

        var note = new Note(project.Id, title, markdown, DateTimeOffset.UtcNow);
        await notes.AddAsync(note, cancellationToken);
        return note;
    }

    /// <summary>Возвращает список заметок, если проект принадлежит указанному владельцу.</summary>
    public async Task<IReadOnlyList<NoteSummary>?> ListAsync(
        Guid projectId,
        Guid ownerId,
        CancellationToken cancellationToken)
    {
        var project = await projects.FindByOwnerAsync(projectId, ownerId, cancellationToken);
        if (project is null)
            return null;

        return await notes.ListAsync(project.Id, cancellationToken);
    }
}
