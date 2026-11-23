using System.ComponentModel.DataAnnotations;

namespace VectraPKM.Projects.Api.Contracts;

/// <summary>Данные для создания личного проекта.</summary>
/// <param name="Name">Название проекта.</param>
public sealed record CreateProjectRequest(
    [Required(ErrorMessage = "Укажите название проекта.")] string Name);
