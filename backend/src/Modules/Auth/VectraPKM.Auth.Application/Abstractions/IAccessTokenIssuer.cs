using VectraPKM.Auth.Application.Contracts;

namespace VectraPKM.Auth.Application.Abstractions;

/// <summary>Выпускает подписанный access-токен для пользователя.</summary>
public interface IAccessTokenIssuer
{
    /// <summary>Создаёт токен с идентификаторами пользователя и сессии и ограниченным сроком действия.</summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="sessionId">Идентификатор сессии; сохраняется при будущем обновлении токенов.</param>
    IssuedAccessToken Issue(Guid userId, Guid sessionId);
}
