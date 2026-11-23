using System.ComponentModel.DataAnnotations;

namespace VectraPKM.Projects.Api.Contracts;

/// <summary>Новый заголовок и текст существующей заметки.</summary>
/// <param name="Title">Заголовок заметки.</param>
/// <param name="Markdown">Исходный Markdown-текст. Пустая строка очищает текст заметки.</param>
public sealed record UpdateNoteRequest(
    [Required(ErrorMessage = "Укажите заголовок заметки.")] string Title,
    [Required(AllowEmptyStrings = true, ErrorMessage = "Укажите Markdown-текст заметки.")] string Markdown);
