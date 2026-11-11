namespace VectraPKM.Auth.Application.Contracts;

/// <summary>Данные для входа по имени пользователя или email и паролю.</summary>
public sealed record AuthenticationData(string Login, string Password);
