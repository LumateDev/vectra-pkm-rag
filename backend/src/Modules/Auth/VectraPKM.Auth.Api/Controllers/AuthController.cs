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
