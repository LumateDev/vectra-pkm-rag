using Microsoft.AspNetCore.Identity;

namespace VectraPKM.Auth.Infrastructure.Identity;

/// <summary>
/// Пользователь ASP.NET Core Identity с идентификатором UUID.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// Создаёт пользователя с новым идентификатором.
    /// </summary>
    public ApplicationUser()
    {
        Id = Guid.NewGuid();
    }
}
