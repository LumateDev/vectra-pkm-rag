namespace VectraPKM.Auth.Application.Contracts;

/// <summary>Исходное значение refresh-токена для cookie и абсолютный срок его действия.</summary>
public sealed record IssuedRefreshToken(string Value, DateTimeOffset ExpiresAt);
