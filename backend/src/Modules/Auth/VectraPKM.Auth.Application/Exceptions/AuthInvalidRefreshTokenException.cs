namespace VectraPKM.Auth.Application.Exceptions;

/// <summary>Refresh-токен отсутствует, истёк, отозван или предъявлен повторно.</summary>
public sealed class AuthInvalidRefreshTokenException() : Exception("Недействительный refresh-токен.");
