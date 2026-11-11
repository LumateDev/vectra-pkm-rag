namespace VectraPKM.Auth.Application.Contracts;

/// <summary>Открытые данные пользователя, прошедшего проверку пароля.</summary>
public sealed record AuthenticationResult(Guid UserId, string Email, string UserName);
