namespace VectraPKM.Auth.Application.Exceptions;

/// <summary>Отказ во входе без раскрытия существования учётной записи или её блокировки.</summary>
public sealed class AuthUnauthorizedException : Exception
{
    /// <summary>Создаёт исключение с общим сообщением для неудачного входа.</summary>
    public AuthUnauthorizedException() : base("Неверный логин или пароль.")
    {
    }
}
