namespace VectraPKM.Auth.Application.Contracts;

/// <summary>
/// Данные для создания учётной записи без автоматического входа.
/// </summary>
/// <param name="Email">Email пользователя.</param>
/// <param name="UserName">Уникальное имя для входа.</param>
/// <param name="Password">Пароль без обрезания пробелов.</param>
public sealed record RegistrationData(string Email, string UserName, string Password);
