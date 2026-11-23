namespace VectraPKM.Projects.Application;

/// <summary>Данные заметки для списка внутри проекта.</summary>
/// <param name="Id">Идентификатор заметки.</param>
/// <param name="Title">Заголовок заметки.</param>
/// <param name="CreatedAt">Время создания в UTC.</param>
/// <param name="UpdatedAt">Время последнего изменения в UTC.</param>
public sealed record NoteSummary(Guid Id, string Title, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
