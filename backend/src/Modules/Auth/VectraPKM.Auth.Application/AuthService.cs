using VectraPKM.Auth.Application.Abstractions;
using VectraPKM.Auth.Application.Contracts;

namespace VectraPKM.Auth.Application;

/// <summary>
/// Реализует сценарии Auth, не обращаясь напрямую к Identity, EF Core и HTTP.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IUserRegistration _userRegistration;

    /// <summary>
    /// Создаёт сервис с инфраструктурной реализацией регистрации пользователей.
    /// </summary>
    public AuthService(IUserRegistration userRegistration)
    {
        _userRegistration = userRegistration;
    }

    /// <inheritdoc />
    public Task<RegistrationResult> RegisterAsync(RegistrationData data, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        return _userRegistration.RegisterAsync(data, cancellationToken);
    }
}
