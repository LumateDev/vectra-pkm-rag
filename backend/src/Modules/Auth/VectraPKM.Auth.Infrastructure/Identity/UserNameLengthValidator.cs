using Microsoft.AspNetCore.Identity;

namespace VectraPKM.Auth.Infrastructure.Identity;

/// <summary>
/// Проверяет длину имени пользователя в дополнение к стандартным проверкам Identity.
/// </summary>
internal sealed class UserNameLengthValidator : IUserValidator<ApplicationUser>
{
    /// <inheritdoc />
    public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
    {
        // Обязательность, допустимые символы и уникальность проверяет стандартный валидатор Identity.
        if (user.UserName is { Length: < 3 or > 32 })
        {
            return Task.FromResult(IdentityResult.Failed(new IdentityError
            {
                Code = "UserNameLength",
                Description = "Имя пользователя должно содержать от 3 до 32 символов."
            }));
        }

        return Task.FromResult(IdentityResult.Success);
    }
}
