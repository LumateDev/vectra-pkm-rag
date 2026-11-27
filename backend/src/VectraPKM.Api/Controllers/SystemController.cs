using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VectraPKM.Shared.Api.Contracts;

namespace VectraPKM.Api.Controllers;

/// <summary>Служебная информация о серверном приложении.</summary>
[Authorize]
[ApiController]
[Route("api/v1/system")]
[Produces("application/json")]
public sealed class SystemController : ControllerBase
{
    /// <summary>Возвращает версию сборки API.</summary>
    [HttpGet("version", Name = "getVersion")]
    [ProducesResponseType<ApiResult<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    public ActionResult<ApiResult<string>> GetVersion()
    {
        var version = typeof(SystemController).Assembly.GetName().Version?.ToString(3) ?? "unknown";
        return Ok(new ApiResult<string>(version));
    }
}
