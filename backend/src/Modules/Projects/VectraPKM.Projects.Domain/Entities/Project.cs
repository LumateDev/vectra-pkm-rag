namespace VectraPKM.Projects.Domain.Entities;

/// <summary>Личный проект, принадлежащий одному пользователю.</summary>
public sealed class Project
{
    /// <summary>Идентификатор проекта.</summary>
    public Guid Id { get; private set; }

    /// <summary>Идентификатор владельца проекта.</summary>
    public Guid OwnerId { get; private set; }

    /// <summary>Название проекта.</summary>
    public string Name { get; private set; }

    /// <summary>Время создания проекта в UTC.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Время последнего изменения проекта в UTC.</summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Создаёт личный проект.</summary>
    /// <param name="ownerId">Идентификатор владельца.</param>
    /// <param name="name">Название проекта.</param>
    /// <param name="createdAt">Время создания.</param>
    public Project(Guid ownerId, string name, DateTimeOffset createdAt)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(ownerId, Guid.Empty);
        Id = Guid.NewGuid();
        OwnerId = ownerId;
        Name = NormalizeName(name);
        CreatedAt = createdAt.ToUniversalTime();
        UpdatedAt = CreatedAt;
    }

    /// <summary>Обновляет название проекта и время его изменения.</summary>
    /// <param name="name">Новое название проекта.</param>
    /// <param name="updatedAt">Время изменения.</param>
    public void Rename(string name, DateTimeOffset updatedAt)
    {
        Name = NormalizeName(name);
        UpdatedAt = updatedAt.ToUniversalTime();
    }

    private static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return name.Trim();
    }
}
