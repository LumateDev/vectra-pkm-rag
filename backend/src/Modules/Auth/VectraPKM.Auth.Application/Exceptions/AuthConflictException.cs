namespace VectraPKM.Auth.Application.Exceptions;

/// <summary>Конфликт данных учётной записи, например занятый email или username.</summary>
public sealed class AuthConflictException : Exception
{
    /// <summary>Создаёт исключение с безопасным для показа пользователю сообщением.</summary>
    public AuthConflictException(string message) : base(message)
    {
    }
}
