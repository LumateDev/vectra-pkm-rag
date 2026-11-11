namespace VectraPKM.Auth.Api.Contracts;

/// <summary>
/// Данные зарегистрированного пользователя. Регистрация не выдаёт токены.
/// </summary>
/// <param name="UserId">Идентификатор учётной записи.</param>
/// <param name="Email">Email пользователя.</param>
/// <param name="UserName">Имя пользователя.</param>
public sealed record RegisterResponse(Guid UserId, string Email, string UserName);
