using System.Security.Claims;

namespace VectraPKM.Projects.Api.Extensions;

internal static class ProjectPrincipalExtensions
{
    internal static bool TryGetOwnerId(this ClaimsPrincipal user, out Guid ownerId)
        => Guid.TryParse(user.FindFirstValue("sub"), out ownerId) && ownerId != Guid.Empty;
}
