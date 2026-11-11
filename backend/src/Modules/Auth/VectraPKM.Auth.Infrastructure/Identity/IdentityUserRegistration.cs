using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VectraPKM.Auth.Application.Abstractions;
using VectraPKM.Auth.Application.Contracts;
using VectraPKM.Auth.Application.Exceptions;

namespace VectraPKM.Auth.Infrastructure.Identity;

/// <summary>
/// Создаёт пользователей через Identity и переводит его ошибки в прикладные исключения.
/// </summary>
internal sealed class IdentityUserRegistration : IUserRegistration
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityUserRegistration(UserManager<ApplicationUser> users)
    {
        _userManager = users;
    }

    /// <inheritdoc />
    public async Task<RegistrationResult> RegisterAsync(RegistrationData data, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = new ApplicationUser { Email = data.Email, UserName = data.UserName };

        IdentityResult result;
        try
        {
            result = await _userManager.CreateAsync(user, data.Password);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "EmailIndex" or "UserNameIndex"
        } postgresException)
        {
            // Два запроса могут одновременно пройти проверку Identity. Последнюю проверку делает уникальный индекс БД.
            throw new AuthConflictException(postgresException.ConstraintName == "EmailIndex"
                ? "Этот email уже используется."
                : "Это имя пользователя уже используется.");
        }

        if (!result.Succeeded)
        {
            var conflict = result.Errors.FirstOrDefault(error => error.Code is "DuplicateEmail" or "DuplicateUserName");
            if (conflict is not null)
            {
                throw new AuthConflictException(MapErrorMessage(conflict));
            }

            throw new AuthValidationException(string.Join(" ", result.Errors.Select(MapErrorMessage).Distinct()));
        }

        return new RegistrationResult(user.Id, user.Email!, user.UserName!);
    }

    private static string MapErrorMessage(IdentityError error)
    {
        return error.Code switch
        {
            "DuplicateEmail" => "Этот email уже используется.",
            "DuplicateUserName" => "Это имя пользователя уже используется.",
            "InvalidEmail" => "Укажите корректный email.",
            "InvalidUserName" => "Имя пользователя должно содержать только латинские буквы, цифры и символы «._-».",
            _ => error.Description
        };
    }
}
