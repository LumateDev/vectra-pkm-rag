namespace VectraPKM.Projects.Api.Contracts;

/// <summary>Сохранённая Markdown-заметка.</summary>
/// <param name="Id">Идентификатор заметки.</param>
/// <param name="ProjectId">Идентификатор проекта.</param>
/// <param name="Title">Заголовок заметки.</param>
/// <param name="Markdown">Исходный Markdown-текст.</param>
/// <param name="CreatedAt">Время создания в UTC.</param>
/// <param name="UpdatedAt">Время последнего изменения в UTC.</param>
public sealed record NoteResponse(
    Guid Id,
    Guid ProjectId,
    string Title,
    string Markdown,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
