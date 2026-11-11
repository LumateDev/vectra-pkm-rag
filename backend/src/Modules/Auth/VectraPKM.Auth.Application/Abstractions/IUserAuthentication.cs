using VectraPKM.Auth.Application.Contracts;

namespace VectraPKM.Auth.Application.Abstractions;

/// <summary>Проверяет учётные данные и ограничения входа в хранилище пользователей.</summary>
public interface IUserAuthentication
{
    /// <summary>Проверяет имя пользователя или email и пароль с учётом блокировки.</summary>
    Task<AuthenticationResult> AuthenticateAsync(AuthenticationData data, CancellationToken cancellationToken = default);
}
