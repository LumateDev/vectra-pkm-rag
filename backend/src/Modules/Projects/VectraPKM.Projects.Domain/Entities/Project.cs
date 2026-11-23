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
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = Guid.NewGuid();
        OwnerId = ownerId;
        Name = name.Trim();
        CreatedAt = createdAt.ToUniversalTime();
        UpdatedAt = CreatedAt;
    }
}
