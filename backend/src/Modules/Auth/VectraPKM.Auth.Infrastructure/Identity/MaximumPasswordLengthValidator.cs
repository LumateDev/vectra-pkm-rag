using Microsoft.AspNetCore.Identity;

namespace VectraPKM.Auth.Infrastructure.Identity;

/// <summary>
/// Ограничивает длину пароля сверху в дополнение к стандартному валидатору Identity.
/// </summary>
internal sealed class MaximumPasswordLengthValidator : IPasswordValidator<ApplicationUser>
{
    /// <inheritdoc />
    public Task<IdentityResult> ValidateAsync(
        UserManager<ApplicationUser> manager,
        ApplicationUser user,
        string? password)
    {
        // Минимальную длину и отсутствие пароля проверяет стандартный валидатор Identity.
        if (password is { Length: > 128 })
        {
            return Task.FromResult(IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordTooLong",
                Description = "Пароль должен содержать не более 128 символов."
            }));
        }

        return Task.FromResult(IdentityResult.Success);
    }
}
