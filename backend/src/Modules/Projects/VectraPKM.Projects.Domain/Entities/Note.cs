namespace VectraPKM.Projects.Domain.Entities;

/// <summary>Markdown-заметка внутри личного проекта.</summary>
public sealed class Note
{
    /// <summary>Идентификатор заметки.</summary>
    public Guid Id { get; private set; }

    /// <summary>Идентификатор проекта, которому принадлежит заметка.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Заголовок заметки.</summary>
    public string Title { get; private set; }

    /// <summary>Исходный Markdown-текст заметки.</summary>
    public string Markdown { get; private set; }

    /// <summary>Время создания заметки в UTC.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Время последнего изменения заметки в UTC.</summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Создаёт заметку в указанном проекте.</summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="title">Непустой заголовок заметки.</param>
    /// <param name="markdown">Текст заметки. Для новой заметки может быть пустым.</param>
    /// <param name="createdAt">Время создания.</param>
    public Note(Guid projectId, string title, string markdown, DateTimeOffset createdAt)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(projectId, Guid.Empty);
        ArgumentNullException.ThrowIfNull(markdown);

        Id = Guid.NewGuid();
        ProjectId = projectId;
        Title = NormalizeTitle(title);
        Markdown = markdown;
        CreatedAt = createdAt.ToUniversalTime();
        UpdatedAt = CreatedAt;
    }

    /// <summary>Обновляет заголовок, текст и время изменения заметки.</summary>
    /// <param name="title">Новый заголовок заметки.</param>
    /// <param name="markdown">Новый текст заметки.</param>
    /// <param name="updatedAt">Время изменения.</param>
    public void Update(string title, string markdown, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        Title = NormalizeTitle(title);
        Markdown = markdown;
        UpdatedAt = updatedAt.ToUniversalTime();
    }

    private static string NormalizeTitle(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        return title.Trim();
    }
}
