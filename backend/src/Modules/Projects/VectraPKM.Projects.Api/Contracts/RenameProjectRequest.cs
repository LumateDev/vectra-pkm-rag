using System.ComponentModel.DataAnnotations;

namespace VectraPKM.Projects.Api.Contracts;

/// <summary>Данные для переименования личного проекта.</summary>
/// <param name="Name">Новое название проекта.</param>
public sealed record RenameProjectRequest(
    [Required(ErrorMessage = "Укажите название проекта.")] string Name);
