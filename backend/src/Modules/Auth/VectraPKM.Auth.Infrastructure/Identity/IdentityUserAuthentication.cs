using Microsoft.AspNetCore.Identity;
using VectraPKM.Auth.Application.Abstractions;
using VectraPKM.Auth.Application.Contracts;
using VectraPKM.Auth.Application.Exceptions;

namespace VectraPKM.Auth.Infrastructure.Identity;

/// <summary>
/// Проверяет пароль через Identity и учитывает неудачные попытки входа.
/// </summary>
internal sealed class IdentityUserAuthentication: IUserAuthentication
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityUserAuthentication(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    /// <inheritdoc />
    public async Task<AuthenticationResult> AuthenticateAsync(AuthenticationData data, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // В имени пользователя символ @ запрещён правилами регистрации.
        var user = data.Login.Contains('@')
            ? await _userManager.FindByEmailAsync(data.Login)
            : await _userManager.FindByNameAsync(data.Login);
        if (user is null || await _userManager.IsLockedOutAsync(user))
        {
            throw new AuthUnauthorizedException();
        }

        if (!await _userManager.CheckPasswordAsync(user, data.Password))
        {
            await _userManager.AccessFailedAsync(user);
            throw new AuthUnauthorizedException();
        }

        var reset = await _userManager.ResetAccessFailedCountAsync(user);
        if (!reset.Succeeded)
        {
            throw new AuthUnauthorizedException();
        }

        return new AuthenticationResult(user.Id, user.Email!, user.UserName!);
    }
}
