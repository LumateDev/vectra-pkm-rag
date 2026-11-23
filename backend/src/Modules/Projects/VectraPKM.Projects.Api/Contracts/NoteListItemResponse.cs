namespace VectraPKM.Projects.Api.Contracts;

/// <summary>Данные заметки для списка.</summary>
/// <param name="Id">Идентификатор заметки.</param>
/// <param name="Title">Заголовок заметки.</param>
/// <param name="CreatedAt">Время создания в UTC.</param>
/// <param name="UpdatedAt">Время последнего изменения в UTC.</param>
public sealed record NoteListItemResponse(Guid Id, string Title, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
