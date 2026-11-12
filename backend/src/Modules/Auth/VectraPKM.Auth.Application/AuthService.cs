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
    private readonly IAccessTokenIssuer _accessTokenIssuer;

    /// <summary>
    /// Создаёт сервис с инфраструктурными реализациями регистрации и проверки учётных данных.
    /// </summary>
    public AuthService(IUserRegistration userRegistration, IUserAuthentication userAuthentication,
        IAccessTokenIssuer accessTokenIssuer)
    {
        _userRegistration = userRegistration;
        _userAuthentication = userAuthentication;
        _accessTokenIssuer = accessTokenIssuer;
    }

    /// <inheritdoc />
    public async Task<SignInResult> AuthAsync(AuthenticationData data, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        var user = await _userAuthentication.AuthenticateAsync(data, cancellationToken);
        var sessionId = Guid.NewGuid();
        return new SignInResult(user, _accessTokenIssuer.Issue(user.UserId, sessionId));
    }

    /// <inheritdoc />
    public Task<RegistrationResult> RegisterAsync(RegistrationData data, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        return _userRegistration.RegisterAsync(data, cancellationToken);
    }
}
