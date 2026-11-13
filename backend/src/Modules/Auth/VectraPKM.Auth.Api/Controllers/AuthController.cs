using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VectraPKM.Auth.Api.Contracts;
using VectraPKM.Auth.Application;
using VectraPKM.Auth.Application.Contracts;
using VectraPKM.Auth.Application.Exceptions;
using VectraPKM.Shared.Api.Contracts;

namespace VectraPKM.Auth.Api.Controllers;

/// <summary>
/// HTTP-сценарии регистрации и аутентификации.
/// </summary>
[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private const string RefreshCookieName = "refreshToken";
    private readonly IAuthService _authService;

    /// <summary>Создаёт контроллер с прикладным сервисом Auth.</summary>
    public AuthController(IAuthService auth)
    {
        _authService = auth;
    }

    /// <summary>
    /// Проверяет имя пользователя или email и пароль и выдаёт JWT с настроенным сроком действия.
    /// </summary>
    /// <param name="request">Имя пользователя или email и пароль пользователя.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<ApiResult<AuthResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<AuthResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResult<AuthResponse>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResult<AuthResponse>>> Auth(
        [FromBody] AuthRequest request,
        CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";
        try
        {
            var result = await _authService.AuthAsync(new AuthenticationData(request.Login, request.Password), cancellationToken);
            var userData = new AuthUserResponse(result.User.UserId, result.User.Email, result.User.UserName);
            var response = new AuthResponse(result.Token.Value, result.Token.ExpiresAt, userData);
            SetRefreshTokenCookie(result.RefreshToken);
            return Ok(new ApiResult<AuthResponse>(response));
        }
        catch (AuthUnauthorizedException exception)
        {
            return Unauthorized(new ApiResult<AuthResponse>(error: exception.Message));
        }
    }

    /// <summary>
    /// Создаёт учётную запись без автоматического входа и выдачи токенов.
    /// </summary>
    /// <param name="request">Email, имя пользователя и пароль.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType<ApiResult<RegisterResponse>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResult<RegisterResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResult<RegisterResponse>>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResult<RegisterResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";
        try
        {
            var result = await _authService.RegisterAsync(
                new RegistrationData(request.Email, request.UserName, request.Password), cancellationToken);

            var response = new RegisterResponse(result.UserId, result.Email, result.UserName);
            return StatusCode(StatusCodes.Status201Created, new ApiResult<RegisterResponse>(response));
        }
        catch (AuthValidationException exception)
        {
            return BadRequest(new ApiResult<RegisterResponse>(error: exception.Message));
        }
        catch (AuthConflictException exception)
        {
            return Conflict(new ApiResult<RegisterResponse>(error: exception.Message));
        }
    }

    /// <summary>Обновляет access и refresh по HttpOnly-cookie без продления абсолютного срока сессии.</summary>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType<ApiResult<AuthResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<AuthResponse>>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResult<AuthResponse>>> Refresh(CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";
        try
        {
            if (!Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken))
                throw new AuthInvalidRefreshTokenException();

            var result = await _authService.RefreshAsync(refreshToken, cancellationToken);
            var user = new AuthUserResponse(result.User.UserId, result.User.Email, result.User.UserName);
            var response = new AuthResponse(result.Token.Value, result.Token.ExpiresAt, user);
            SetRefreshTokenCookie(result.RefreshToken);
            return Ok(new ApiResult<AuthResponse>(response));
        }
        catch (AuthInvalidRefreshTokenException exception)
        {
            ClearRefreshTokenCookie();
            return Unauthorized(new ApiResult<AuthResponse>(error: exception.Message));
        }
    }

    /// <summary>Отзывает refresh-токены текущей сессии и удаляет cookie. Access действует до exp.</summary>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType<ApiResult<bool>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResult<bool>>> Logout(CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";
        Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken);
        await _authService.LogoutAsync(refreshToken, cancellationToken);
        ClearRefreshTokenCookie();
        return Ok(new ApiResult<bool>(true));
    }

    

    private static CookieOptions CreateRefreshCookieOptions() => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = "/auth"
    };

    private void SetRefreshTokenCookie(IssuedRefreshToken refreshToken)
    {
        var options = CreateRefreshCookieOptions();
        options.Expires = refreshToken.ExpiresAt;
        options.MaxAge = refreshToken.ExpiresAt - DateTimeOffset.UtcNow;
        Response.Cookies.Append(RefreshCookieName, refreshToken.Value, options);
    }


    private void ClearRefreshTokenCookie()
    => Response.Cookies.Delete(RefreshCookieName, CreateRefreshCookieOptions());
}
