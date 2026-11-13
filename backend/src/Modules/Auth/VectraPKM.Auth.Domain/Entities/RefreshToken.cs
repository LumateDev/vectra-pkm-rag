namespace VectraPKM.Auth.Domain.Entities;

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

    private RefreshToken() { }

    /// <summary>
    /// Создаёт запись хеша токена для сессии с фиксированным сроком окончания.
    /// </summary>
    public RefreshToken(Guid userId, Guid sessionId, string tokenHash, DateTimeOffset createdAt, DateTimeOffset expiresAt)
    {
        if (userId == Guid.Empty || sessionId == Guid.Empty)
            throw new ArgumentException("Идентификаторы пользователя и сессии не должны быть пустыми.");
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        if (expiresAt <= createdAt)
            throw new ArgumentException("Срок действия должен заканчиваться после создания токена.");

        Id = Guid.NewGuid();
        UserId = userId;
        SessionId = sessionId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    /// <summary>
    /// Помечает действующий токен использованным для ротации.
    /// </summary>
    public void MarkRefreshed(DateTimeOffset now)
    {
        if (RevokedAt is not null || RefreshedAt is not null || ExpiresAt <= now)
            throw new InvalidOperationException("Токен нельзя использовать для обновления.");
        RefreshedAt = now;
    }
}
