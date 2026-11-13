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
    private readonly IRefreshTokenService _refreshTokens;

    /// <summary>
    /// Создаёт сервис с инфраструктурными реализациями регистрации и проверки учётных данных.
    /// </summary>
    public AuthService(IUserRegistration userRegistration, IUserAuthentication userAuthentication,
        IAccessTokenIssuer accessTokenIssuer, IRefreshTokenService refreshTokens)
    {
        _userRegistration = userRegistration;
        _userAuthentication = userAuthentication;
        _accessTokenIssuer = accessTokenIssuer;
        _refreshTokens = refreshTokens;
    }

    /// <inheritdoc />
    public async Task<SignInResult> AuthAsync(AuthenticationData data, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        var user = await _userAuthentication.AuthenticateAsync(data, cancellationToken);
        var sessionId = Guid.NewGuid();
        var accessToken = _accessTokenIssuer.Issue(user.UserId, sessionId);
        var refreshToken = await _refreshTokens.CreateAsync(user.UserId, sessionId, cancellationToken);
        return new SignInResult(user, accessToken, refreshToken);
    }

    /// <inheritdoc />
    public Task<RegistrationResult> RegisterAsync(RegistrationData data, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        return _userRegistration.RegisterAsync(data, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SignInResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var session = await _refreshTokens.RotateAsync(refreshToken, cancellationToken);
        var rotatedToken = _accessTokenIssuer.Issue(session.User.UserId, session.SessionId);
        return new SignInResult(session.User, rotatedToken, session.RefreshToken);
    }

    /// <inheritdoc />
    public Task LogoutAsync(string? refreshToken, CancellationToken cancellationToken = default)
        => _refreshTokens.RevokeAsync(refreshToken, cancellationToken);
}
