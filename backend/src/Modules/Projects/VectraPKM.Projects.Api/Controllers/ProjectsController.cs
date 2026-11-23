using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VectraPKM.Projects.Api.Contracts;
using VectraPKM.Projects.Application;
using VectraPKM.Projects.Domain.Entities;
using VectraPKM.Shared.Api.Contracts;

namespace VectraPKM.Projects.Api.Controllers;

/// <summary>API личных проектов текущего пользователя.</summary>
[ApiController]
[Authorize]
[Route("api/projects")]
public sealed class ProjectsController(ProjectService projectService) : ControllerBase
{
    /// <summary>Создаёт личный проект текущего пользователя.</summary>
    [HttpPost(Name = "createProject")]
    [ProducesResponseType<ApiResult<ProjectResponse>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResult<ProjectResponse>>> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
            return Challenge();

        var project = await projectService.CreateAsync(ownerId, request.Name, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new ApiResult<ProjectResponse>(ToResponse(project)));
    }

    /// <summary>Возвращает список личных проектов текущего пользователя.</summary>
    [HttpGet(Name = "listProjects")]
    [ProducesResponseType<ApiResult<IReadOnlyList<ProjectResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResult<IReadOnlyList<ProjectResponse>>>> List(
        CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
            return Challenge();

        var projects = await projectService.ListAsync(ownerId, cancellationToken);
        return Ok(new ApiResult<IReadOnlyList<ProjectResponse>>(projects.Select(ToResponse).ToArray()));
    }

    private bool TryGetOwnerId(out Guid ownerId)
        => Guid.TryParse(User.FindFirstValue("sub"), out ownerId) && ownerId != Guid.Empty;

    private static ProjectResponse ToResponse(Project project)
        => new(project.Id, project.Name, project.CreatedAt, project.UpdatedAt);
}
