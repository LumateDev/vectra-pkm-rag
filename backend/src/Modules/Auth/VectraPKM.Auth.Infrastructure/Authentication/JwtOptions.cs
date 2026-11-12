using System.Text;

namespace VectraPKM.Auth.Infrastructure.Authentication;

/// <summary>Настройки JWT из секции Jwt в appsettings.json.</summary>
public sealed class JwtOptions
{
    /// <summary>Имя секции конфигурации.</summary>
    public const string SectionName = "Jwt";

    /// <summary>Общий ключ выпуска и проверки подписи HS256, минимум 32 байта UTF-8.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Время жизни access-токена в минутах.</summary>
    public int AccessTokenLifetimeMinutes { get; set; } = 15;

    /// <summary>Время жизни refresh-токена в днях. Будет использовано при реализации refresh.</summary>
    public int RefreshTokenLifetimeDays { get; set; } = 30;

    /// <summary>Интервал очистки токенов в часах. Будет использован при реализации очистки.</summary>
    public int TokenCleanupIntervalHours { get; set; } = 1;

    /// <summary>Издатель токена.</summary>
    public string Issuer { get; set; } = "VectraPKM.Api";

    /// <summary>API, которому предназначен токен.</summary>
    public string Audience { get; set; } = "VectraPKM.Client";

    /// <summary>Проверяет наличие ключа, издателя, получателя и положительные сроки.</summary>
    /// <exception cref="InvalidOperationException">Настройки отсутствуют или некорректны.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Key) || Encoding.UTF8.GetByteCount(Key) < 32)
            throw new InvalidOperationException("Jwt:Key должен содержать не менее 32 байт UTF-8 для HS256.");

        if (string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("Необходимо задать Jwt:Issuer и Jwt:Audience.");

        if (AccessTokenLifetimeMinutes <= 0 || RefreshTokenLifetimeDays <= 0 || TokenCleanupIntervalHours <= 0)
            throw new InvalidOperationException("Сроки жизни и интервал очистки в Jwt должны быть положительными.");
    }
}
