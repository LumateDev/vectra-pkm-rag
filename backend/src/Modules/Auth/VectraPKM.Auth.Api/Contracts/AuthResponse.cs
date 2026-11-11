namespace VectraPKM.Auth.Api.Contracts;

/// <summary>Токен доступа после успешной проверки учётных данных.</summary>
/// <param name="AccessToken">Временная заглушка; не является действительным токеном доступа.</param>
public sealed record AuthResponse(string AccessToken);
