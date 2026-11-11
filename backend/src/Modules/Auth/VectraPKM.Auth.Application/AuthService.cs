using VectraPKM.Auth.Application.Abstractions;
using VectraPKM.Auth.Application.Contracts;

namespace VectraPKM.Auth.Application;

/// <summary>
/// Реализует сценарии Auth, не обращаясь напрямую к Identity, EF Core и HTTP.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IUserRegistration _userRegistration;
    private readonly IUserAuthentication _userAuthentication;

    /// <summary>
    /// Создаёт сервис с инфраструктурными реализациями регистрации и проверки учётных данных.
    /// </summary>
    public AuthService(IUserRegistration userRegistration, IUserAuthentication userAuthentication)
    {
        _userRegistration = userRegistration;
        _userAuthentication = userAuthentication;
    }

    /// <inheritdoc />
    public Task<AuthenticationResult> AuthAsync(AuthenticationData data, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        return _userAuthentication.AuthenticateAsync(data, cancellationToken);
    }

    /// <inheritdoc />
    public Task<RegistrationResult> RegisterAsync(RegistrationData data, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        return _userRegistration.RegisterAsync(data, cancellationToken);
    }
}
