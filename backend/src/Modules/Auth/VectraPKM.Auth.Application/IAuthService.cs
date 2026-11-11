using VectraPKM.Auth.Application.Contracts;

namespace VectraPKM.Auth.Application;

/// <summary>
/// Сценарии аутентификации и управления учётной записью.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Регистрирует пользователя без создания сессии и выдачи токенов.
    /// </summary>
    /// <param name="data">Email, имя пользователя и пароль.</param>
    /// <param name="cancellationToken">Токен отмены до начала операции.</param>
    Task<RegistrationResult> RegisterAsync(RegistrationData data, CancellationToken cancellationToken = default);
}
