namespace VectraPKM.Auth.Application.Exceptions;

/// <summary>Ошибка входных данных прикладного сценария Auth, не связанная с HTTP.</summary>
public sealed class AuthValidationException : Exception
{
    /// <summary>Создаёт исключение с безопасным для показа пользователю сообщением.</summary>
    public AuthValidationException(string message) : base(message)
    {
    }
}
