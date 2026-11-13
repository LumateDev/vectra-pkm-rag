using VectraPKM.Auth.Application.Contracts;

namespace VectraPKM.Auth.Application.Abstractions;

/// <summary>
/// Создаёт, обновляет и отзывает семейства refresh-токенов.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Создаёт refresh-токен для нового входа.
    /// </summary>
    Task<IssuedRefreshToken> CreateAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Однократно использует refresh-токен и возвращает продолжение той же сессии.
    /// </summary>
    Task<RefreshedSession> RotateAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отзывает семейство по любому его refresh-токену; отсутствие или неизвестное значение допустимы.
    /// </summary>
    Task RevokeAsync(string? refreshToken, CancellationToken cancellationToken = default);
}
