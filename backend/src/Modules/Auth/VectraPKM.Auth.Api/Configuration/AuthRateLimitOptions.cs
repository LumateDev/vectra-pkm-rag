namespace VectraPKM.Auth.Api.Configuration;

/// <summary>Настройки ограничений auth-запросов по IP из секции AuthRateLimiting.</summary>
public sealed class AuthRateLimitOptions
{
    /// <summary>Имя секции конфигурации.</summary>
    public const string SectionName = "AuthRateLimiting";

    /// <summary>Лимит входа: по умолчанию 10 запросов за минуту.</summary>
    public EndpointRateLimitOptions Login { get; set; } = new() { PermitLimit = 10 };

    /// <summary>Лимит регистрации: по умолчанию 5 запросов за 10 минут.</summary>
    public EndpointRateLimitOptions Register { get; set; } = new() { PermitLimit = 5, WindowSeconds = 600 };

    /// <summary>Лимит обновления токенов: по умолчанию 10 запросов за минуту.</summary>
    public EndpointRateLimitOptions Refresh { get; set; } = new() { PermitLimit = 10 };

    /// <summary>Лимит выхода: по умолчанию 5 запросов за минуту.</summary>
    public EndpointRateLimitOptions Logout { get; set; } = new() { PermitLimit = 5 };

    /// <summary>Проверяет положительные значения лимитов и длительности окон.</summary>
    public void Validate()
    {
        ValidateEndpoint(Login, nameof(Login));
        ValidateEndpoint(Register, nameof(Register));
        ValidateEndpoint(Refresh, nameof(Refresh));
        ValidateEndpoint(Logout, nameof(Logout));
    }

    private static void ValidateEndpoint(EndpointRateLimitOptions? options, string name)
    {
        if (options is null || options.PermitLimit <= 0 || options.WindowSeconds <= 0)
            throw new InvalidOperationException($"{SectionName}:{name}: PermitLimit и WindowSeconds должны быть положительными.");
    }
}

/// <summary>Количество разрешённых запросов в фиксированном временном окне.</summary>
public sealed class EndpointRateLimitOptions
{
    /// <summary>Количество разрешённых запросов.</summary>
    public int PermitLimit { get; set; }

    /// <summary>Длительность окна в секундах.</summary>
    public int WindowSeconds { get; set; } = 60;
}
