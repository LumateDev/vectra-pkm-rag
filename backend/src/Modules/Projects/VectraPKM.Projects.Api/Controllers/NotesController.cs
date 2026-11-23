using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VectraPKM.Projects.Api.Contracts;
using VectraPKM.Projects.Api.Extensions;
using VectraPKM.Projects.Application;
using VectraPKM.Projects.Domain.Entities;
using VectraPKM.Shared.Api.Contracts;

namespace VectraPKM.Projects.Api.Controllers;

/// <summary>API заметок личного проекта текущего пользователя.</summary>
[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/notes")]
public sealed class NotesController(NoteService noteService) : ControllerBase
{
    /// <summary>Создаёт Markdown-заметку в личном проекте.</summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="request">Заголовок и текст заметки.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPost(Name = "createNote")]
    [ProducesResponseType<ApiResult<NoteResponse>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResult<NoteResponse>>> Create(
        Guid projectId,
        [FromBody] CreateNoteRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetOwnerId(out var ownerId))
            return Challenge();

        var note = await noteService.CreateAsync(projectId, ownerId, request.Title, request.Markdown, cancellationToken);
        if (note is null)
            return NotFound(new ApiResult<object>(error: "Проект не найден."));

        return CreatedAtRoute(
            "getNote",
            new { projectId = note.ProjectId, noteId = note.Id },
            new ApiResult<NoteResponse>(ToResponse(note)));
    }

    /// <summary>Возвращает список заметок личного проекта.</summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpGet(Name = "listNotes")]
    [ProducesResponseType<ApiResult<IReadOnlyList<NoteListItemResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResult<IReadOnlyList<NoteListItemResponse>>>> List(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetOwnerId(out var ownerId))
            return Challenge();

        var notes = await noteService.ListAsync(projectId, ownerId, cancellationToken);
        if (notes is null)
            return NotFound(new ApiResult<object>(error: "Проект не найден."));

        var response = notes.Select(note => new NoteListItemResponse(
            note.Id, note.Title, note.CreatedAt, note.UpdatedAt)).ToArray();
        return Ok(new ApiResult<IReadOnlyList<NoteListItemResponse>>(response));
    }

    /// <summary>Возвращает заголовок и текст заметки личного проекта.</summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="noteId">Идентификатор заметки.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpGet("{noteId:guid}", Name = "getNote")]
    [ProducesResponseType<ApiResult<NoteResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResult<NoteResponse>>> Get(
        Guid projectId,
        Guid noteId,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetOwnerId(out var ownerId))
            return Challenge();

        var note = await noteService.GetAsync(projectId, noteId, ownerId, cancellationToken);
        return ToNoteResult(note);
    }

    /// <summary>Сохраняет заголовок и текст существующей заметки личного проекта.</summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="noteId">Идентификатор заметки.</param>
    /// <param name="request">Новый заголовок и текст заметки.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPut("{noteId:guid}", Name = "updateNote")]
    [ProducesResponseType<ApiResult<NoteResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResult<NoteResponse>>> Update(
        Guid projectId,
        Guid noteId,
        [FromBody] UpdateNoteRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetOwnerId(out var ownerId))
            return Challenge();

        var note = await noteService.UpdateAsync(
            projectId, noteId, ownerId, request.Title, request.Markdown, cancellationToken);
        return ToNoteResult(note);
    }

    private ActionResult<ApiResult<NoteResponse>> ToNoteResult(Note? note)
    {
        if (note is null)
            return NotFound(new ApiResult<object>(error: "Заметка не найдена."));

        return Ok(new ApiResult<NoteResponse>(ToResponse(note)));
    }

    private static NoteResponse ToResponse(Note note)
        => new(note.Id, note.ProjectId, note.Title, note.Markdown, note.CreatedAt, note.UpdatedAt);
}
