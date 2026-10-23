namespace VectraPKM.Identity.Domain.Entities;

/// <summary>
/// Хеш refresh-токена и сведения о его жизненном цикле.
/// Исходное значение токена не хранится.
/// </summary>
public sealed class RefreshToken
{
    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Идентификатор владельца токена.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Идентификатор семейства токенов, созданного при одном входе.
    /// </summary>
    public Guid SessionId { get; private set; }

    /// <summary>
    /// SHA-256 хеш refresh-токена.
    /// </summary>
    public string TokenHash { get; private set; } = string.Empty;

    /// <summary>
    /// Время выдачи токена в UTC.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Абсолютное время окончания сессии в UTC, общее для всего семейства.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>
    /// Время использования токена для ротации в UTC.
    /// </summary>
    public DateTimeOffset? RefreshedAt { get; private set; }

    /// <summary>
    /// Время отзыва токена в UTC.
    /// </summary>
    public DateTimeOffset? RevokedAt { get; private set; }
}
