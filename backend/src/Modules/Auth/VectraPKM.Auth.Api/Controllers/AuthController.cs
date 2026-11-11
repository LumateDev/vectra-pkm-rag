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
    private readonly IAuthService _authService;

    /// <summary>Создаёт контроллер с прикладным сервисом Auth.</summary>
    public AuthController(IAuthService auth)
    {
        _authService = auth;
    }

    /// <summary>
    /// Проверяет имя пользователя или email и пароль. Поле accessToken пока содержит заглушку temptoken вместо JWT.
    /// </summary>
    /// <param name="request">Имя пользователя или email и пароль пользователя.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPost]
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
            await _authService.AuthAsync(
                new AuthenticationData(request.Login, request.Password), cancellationToken);

            // TODO: выдавать JWT с userId, email и userName из AuthenticationResult вместо заглушки.
            var response = new AuthResponse("temptoken");
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
}
