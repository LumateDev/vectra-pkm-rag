namespace VectraPKM.Auth.Application.Contracts;

/// <summary>Выпущенный токен доступа и срок его действия.</summary>
public sealed record IssuedAccessToken(string Value, DateTimeOffset ExpiresAt);