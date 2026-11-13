using VectraPKM.Auth.Application.Contracts;

namespace VectraPKM.Auth.Application;

/// <summary>
/// Сценарии аутентификации и управления учётной записью.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Проверяет имя пользователя или email и пароль с учётом блокировки.
    /// </summary>
    Task<SignInResult> AuthAsync(AuthenticationData data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Регистрирует пользователя без создания сессии и выдачи токенов.
    /// </summary>
    /// <param name="data">Email, имя пользователя и пароль.</param>
    /// <param name="cancellationToken">Токен отмены до начала операции.</param>
    Task<RegistrationResult> RegisterAsync(RegistrationData data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет пару токенов по refresh-токену без повторного ввода пароля.
    /// </summary>
    Task<SignInResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Завершает сессию по refresh-токену; access действует до своего exp.
    /// </summary>
    Task LogoutAsync(string? refreshToken, CancellationToken cancellationToken = default);
}
