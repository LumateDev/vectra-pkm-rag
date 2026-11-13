namespace VectraPKM.Auth.Application.Contracts;

/// <summary>Пользователь и продолжение существующей сессии после ротации refresh-токена.</summary>
public sealed record RefreshedSession(AuthenticationResult User, Guid SessionId, IssuedRefreshToken RefreshToken);
