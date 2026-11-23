namespace VectraPKM.Projects.Api.Contracts;

/// <summary>Данные личного проекта.</summary>
/// <param name="Id">Идентификатор проекта.</param>
/// <param name="Name">Название проекта.</param>
/// <param name="CreatedAt">Время создания в UTC.</param>
/// <param name="UpdatedAt">Время последнего изменения в UTC.</param>
public sealed record ProjectResponse(Guid Id, string Name, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
