using System.ComponentModel.DataAnnotations;

namespace VectraPKM.Auth.Api.Contracts;

/// <summary>
/// Запрос на регистрацию по email, имени пользователя и паролю.
/// </summary>
public sealed record RegisterRequest
{
    /// <summary>Уникальный email.</summary>
    /// <example>user@example.com</example>
    [Required, EmailAddress, StringLength(256)]
    public required string Email { get; init; }

    /// <summary>Уникальное имя: 3–32 символа, латиница, цифры и символы «._-».</summary>
    /// <example>aboba</example>
    [Required, StringLength(32, MinimumLength = 3), RegularExpression(@"^[a-zA-Z0-9._-]+$")]
    public required string UserName { get; init; }

    /// <summary>Пароль длиной 8–128 символов без изменения пробелов.</summary>
    /// <example>12345678</example>
    [Required, StringLength(128, MinimumLength = 8)]
    public required string Password { get; init; }
}
