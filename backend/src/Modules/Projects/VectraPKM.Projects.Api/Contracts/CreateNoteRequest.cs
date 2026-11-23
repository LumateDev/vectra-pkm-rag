using System.ComponentModel.DataAnnotations;

namespace VectraPKM.Projects.Api.Contracts;

/// <summary>Данные для создания заметки в проекте.</summary>
/// <param name="Title">Заголовок заметки.</param>
/// <param name="Markdown">Исходный Markdown-текст. По умолчанию пустой.</param>
public sealed record CreateNoteRequest(
    [Required(ErrorMessage = "Укажите заголовок заметки.")] string Title,
    [Required(AllowEmptyStrings = true, ErrorMessage = "Markdown должен быть строкой.")] string Markdown = "");
