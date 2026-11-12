namespace VectraPKM.Auth.Api.Contracts;

/// <summary>Токен доступа после успешной проверки учётных данных.</summary>
/// <param name="AccessToken">Подписанный JWT для авторизованных запросов.</param>
/// <param name="ExpiresAt">Абсолютное время окончания действия в UTC.</param>
/// <param name="User">Открытые данные пользователя.</param>
public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt, AuthUserResponse User);

/// <summary>Данные пользователя для интерфейса; не используются как доказательство прав.</summary>
public sealed record AuthUserResponse(Guid Id, string Email, string UserName);
