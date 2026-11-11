using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VectraPKM.Auth.Infrastructure.Identity;
using Xunit;

namespace VectraPKM.Auth.Tests;

/// <summary>
/// Проверки правил имени пользователя через настоящий UserManager и тестовое хранилище в памяти.
/// </summary>
public sealed class IdentityUserNameTests
{
    /// <summary>
    /// Имя ровно из 3 или 32 символов принимается, а созданного пользователя можно найти по имени.
    /// </summary>
    /// <param name="length">Минимальная или максимальная разрешённая длина имени.</param>
    [Theory(DisplayName = "Username: длина 3 и 32 символа разрешена")]
    [InlineData(3)]
    [InlineData(32)]
    public async Task Usernames_at_both_length_boundaries_are_accepted(int length)
    {
        using var services = PasswordTestServices.Create();
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = NewUser(new string('a', length));

        Assert.True((await manager.CreateAsync(user)).Succeeded);
        Assert.Equal(user.Id, (await manager.FindByNameAsync(user.UserName!))!.Id);
    }

    /// <summary>
    /// Имя из 2 или 33 символов отклоняется с ошибкой UserNameLength; пользователь не сохраняется.
    /// </summary>
    /// <param name="length">Длина на один символ меньше минимума или больше максимума.</param>
    [Theory(DisplayName = "Username: длина вне диапазона 3–32 отклоняется")]
    [InlineData(2)]
    [InlineData(33)]
    public async Task Usernames_outside_length_boundaries_are_not_stored(int length)
    {
        using var services = PasswordTestServices.Create();
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var result = await manager.CreateAsync(NewUser(new string('a', length)));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == "UserNameLength");
        Assert.Empty(services.GetRequiredService<InMemoryUserStore>().Users);
    }

    /// <summary>
    /// Отсутствующее имя, email вместо имени, кириллица, пробелы и знак плюс отклоняются.
    /// Проверяется ошибка InvalidUserName и отсутствие пользователя в хранилище.
    /// </summary>
    /// <param name="userName">Один из недопустимых вариантов имени.</param>
    [Theory(DisplayName = "Username: пустое имя и запрещённые символы отклоняются")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("alex@example.com")]
    [InlineData("алекс")]
    [InlineData("alex name")]
    [InlineData(" alex")]
    [InlineData("alex ")]
    [InlineData("alex+test")]
    public async Task Missing_usernames_and_disallowed_characters_are_rejected(string? userName)
    {
        using var services = PasswordTestServices.Create();
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var result = await manager.CreateAsync(NewUser(userName));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == "InvalidUserName");
        Assert.Empty(services.GetRequiredService<InMemoryUserStore>().Users);
    }

    /// <summary>
    /// Латинские буквы, цифры и символы ._- разрешены; введённый регистр имени сохраняется.
    /// </summary>
    [Fact(DisplayName = "Username: допустимые символы и исходный регистр сохраняются")]
    public async Task Latin_letters_digits_and_allowed_punctuation_are_accepted_and_preserved()
    {
        using var services = PasswordTestServices.Create();
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = NewUser("Alex_01.test-user");

        Assert.True((await manager.CreateAsync(user)).Succeeded);
        Assert.Equal("Alex_01.test-user", user.UserName);
    }

    /// <summary>
    /// После создания Alex имя alex считается занятым, а поиск aLeX находит первого пользователя.
    /// </summary>
    [Fact(DisplayName = "Username: уникальность и поиск не зависят от регистра")]
    public async Task Usernames_are_unique_and_searchable_without_case_sensitivity()
    {
        using var services = PasswordTestServices.Create();
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var first = NewUser("Alex");
        Assert.True((await manager.CreateAsync(first)).Succeeded);

        var result = await manager.CreateAsync(NewUser("alex"));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == "DuplicateUserName");
        Assert.Single(services.GetRequiredService<InMemoryUserStore>().Users);
        Assert.Equal(first.Id, (await manager.FindByNameAsync("aLeX"))!.Id);
    }

    /// <summary>
    /// Создаёт пользователя с проверяемым именем и отдельным уникальным email,
    /// чтобы проверке имени не мешало совпадение адресов.
    /// </summary>
    private static ApplicationUser NewUser(string? userName)
    {
        return new ApplicationUser
        {
            UserName = userName,
            Email = Guid.NewGuid().ToString("N") + "@example.com"
        };
    }
}
