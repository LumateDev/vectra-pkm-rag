using System.ComponentModel.DataAnnotations;

namespace VectraPKM.Auth.Api.Contracts;

/// <summary>Запрос на вход по имени пользователя или email и паролю.</summary>
public sealed record AuthRequest
{
    /// <summary>Имя пользователя или email учётной записи.</summary>
    [Required, StringLength(256)]
    public required string Login { get; init; }

    /// <summary>Пароль без изменения пробелов.</summary>
    [Required, StringLength(128)]
    public required string Password { get; init; }
}
