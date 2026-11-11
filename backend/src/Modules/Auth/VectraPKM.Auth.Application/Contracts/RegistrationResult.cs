namespace VectraPKM.Auth.Application.Contracts;

/// <summary>
/// Данные успешно зарегистрированного пользователя, без пароля, хеша и токенов.
/// </summary>
/// <param name="UserId">Идентификатор учётной записи.</param>
/// <param name="Email">Email пользователя.</param>
/// <param name="UserName">Имя пользователя с сохранением введённого регистра.</param>
public sealed record RegistrationResult(Guid UserId, string Email, string UserName);
