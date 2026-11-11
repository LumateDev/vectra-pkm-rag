using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VectraPKM.Auth.Infrastructure.Identity;
using Xunit;

namespace VectraPKM.Auth.Tests;

/// <summary>
/// Проверки хеширования и парольной политики Identity без подключения к БД.
/// </summary>
public sealed class IdentityPasswordTests
{
    /// <summary>
    /// При создании пользователя сохраняется хеш. Исходный пароль подходит,
    /// неверный пароль и исходный пароль без крайних пробелов не подходят.
    /// </summary>
    [Fact(DisplayName = "Пароль: сохраняется хеш, неверный пароль отклоняется, пробелы значимы")]
    public async Task UserManager_stores_only_hash_and_rejects_incorrect_password()
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = NewUser();
        const string password = " pass word ";

        var result = await manager.CreateAsync(user, password);

        Assert.True(result.Succeeded);
        Assert.NotNull(user.PasswordHash);
        Assert.NotEqual(password, user.PasswordHash);
        Assert.True(await manager.CheckPasswordAsync(user, password));
        Assert.False(await manager.CheckPasswordAsync(user, "incorrect password"));
        Assert.False(await manager.CheckPasswordAsync(user, password.Trim()));
    }

    /// <summary>
    /// Пароли ровно из 8 и 128 символов принимаются и затем успешно проходят проверку.
    /// </summary>
    /// <param name="length">Минимальная или максимальная разрешённая длина пароля.</param>
    [Theory(DisplayName = "Пароль: длина 8 и 128 символов разрешена")]
    [InlineData(8)]
    [InlineData(128)]
    public async Task Passwords_at_both_length_boundaries_are_accepted(int length)
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = NewUser();
        var password = new string('a', length);

        Assert.True((await manager.CreateAsync(user, password)).Succeeded);
        Assert.True(await manager.CheckPasswordAsync(user, password));
    }

    /// <summary>
    /// Пароли длиной 0, 7, 129 и 1024 символа отклоняются с ожидаемым кодом ошибки.
    /// Хеш не создаётся, пользователь в хранилище не появляется.
    /// </summary>
    /// <param name="length">Недопустимая длина пароля.</param>
    /// <param name="expectedCode">Ожидаемый код ошибки минимальной или максимальной длины.</param>
    [Theory(DisplayName = "Пароль: недопустимая длина отклоняется до хеширования и сохранения")]
    [InlineData(0, "PasswordTooShort")]
    [InlineData(7, "PasswordTooShort")]
    [InlineData(129, "PasswordTooLong")]
    [InlineData(1024, "PasswordTooLong")]
    public async Task Invalid_lengths_are_rejected_before_hashing_or_storing(int length, string expectedCode)
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = NewUser();

        var result = await manager.CreateAsync(user, new string('a', length));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == expectedCode);
        Assert.Null(user.PasswordHash);
        Assert.Empty(await AuthTestServices.ReadUsersAsync(services));
    }

    /// <summary>
    /// Пароли из одинаковых букв, только цифр, только спецсимволов и пароль с кириллицей
    /// принимаются: обязательных сочетаний классов символов у нас нет.
    /// </summary>
    /// <param name="password">Пароль, не соответствующий стандартным требованиям сложности Identity.</param>
    [Theory(DisplayName = "Пароль: сочетание букв, цифр и спецсимволов не обязательно")]
    [InlineData("aaaaaaaa")]
    [InlineData("AAAAAAAA")]
    [InlineData("12345678")]
    [InlineData("!!!!!!!!")]
    [InlineData(" пароль ")]
    public async Task Character_composition_rules_are_not_required(string password)
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = NewUser();

        Assert.True((await manager.CreateAsync(user, password)).Succeeded);
        Assert.True(await manager.CheckPasswordAsync(user, password));
    }

    /// <summary>
    /// Хеш с 10 000 итераций после успешной проверки заменяется хешем с актуальными параметрами.
    /// Новый контекст видит сохранённый обновлённый хеш, которому не требуется повторное перехеширование.
    /// </summary>
    [Fact(DisplayName = "Пароль: устаревший хеш обновляется после успешной проверки")]
    public async Task Identity_rehashes_an_old_password_hash_after_successful_verification()
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = NewUser();
        var oldHasher = new PasswordHasher<ApplicationUser>(Options.Create(new PasswordHasherOptions { IterationCount = 10_000 }));
        var oldHash = oldHasher.HashPassword(user, "password");
        user.PasswordHash = oldHash;
        Assert.True((await manager.CreateAsync(user)).Succeeded);

        Assert.True(await manager.CheckPasswordAsync(user, "password"));

        Assert.NotEqual(oldHash, user.PasswordHash);
        Assert.Equal(PasswordVerificationResult.Success,
            manager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash!, "password"));
        var storedUser = Assert.Single(await AuthTestServices.ReadUsersAsync(services));
        Assert.Equal(user.PasswordHash, storedUser.PasswordHash);
        Assert.NotEqual(oldHash, storedUser.PasswordHash);
    }

    /// <summary>
    /// В DI зарегистрирован стандартный PasswordHasher Identity, а не собственная реализация.
    /// </summary>
    [Fact(DisplayName = "Пароль: используется встроенный PasswordHasher Identity")]
    public void Identity_uses_the_builtin_password_hasher()
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();

        Assert.IsType<PasswordHasher<ApplicationUser>>(hasher);
    }

    /// <summary>
    /// Один пароль у двух пользователей получает разные хеши благодаря случайной соли.
    /// Оба пользователя при этом успешно проходят проверку исходным паролем.
    /// </summary>
    [Fact(DisplayName = "Пароль: одинаковые пароли получают разные действительные хеши")]
    public async Task Same_password_produces_distinct_hashes_that_both_verify()
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var first = NewUser();
        var second = NewUser();
        const string password = "same password";

        Assert.True((await manager.CreateAsync(first, password)).Succeeded);
        Assert.True((await manager.CreateAsync(second, password)).Succeeded);

        Assert.NotEqual(first.PasswordHash, second.PasswordHash);
        Assert.True(await manager.CheckPasswordAsync(first, password));
        Assert.True(await manager.CheckPasswordAsync(second, password));
    }

    /// <summary>
    /// Исходный пароль с учётом регистра и Unicode подходит,
    /// а вариант с другим регистром или другим emoji не подходит.
    /// </summary>
    /// <param name="password">Исходный пароль.</param>
    /// <param name="differentPassword">Похожий, но отличающийся пароль, который должен быть отклонён.</param>
    [Theory(DisplayName = "Пароль: регистр и символы Unicode значимы")]
    [InlineData("Password", "password")]
    [InlineData("пароль🙂", "пароль🙃")]
    public async Task Verification_preserves_case_and_unicode(string password, string differentPassword)
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = NewUser();

        Assert.True((await manager.CreateAsync(user, password)).Succeeded);
        Assert.True(await manager.CheckPasswordAsync(user, password));
        Assert.False(await manager.CheckPasswordAsync(user, differentPassword));
    }

    /// <summary>
    /// Изменение одного бита в сохранённом хеше приводит к отказу проверки даже с верным паролем.
    /// </summary>
    [Fact(DisplayName = "Пароль: подменённый хеш не проходит проверку")]
    public async Task Tampered_hash_is_rejected()
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = NewUser();
        Assert.True((await manager.CreateAsync(user, "password")).Succeeded);
        var bytes = Convert.FromBase64String(user.PasswordHash!);
        bytes[^1] ^= 1;
        user.PasswordHash = Convert.ToBase64String(bytes);

        Assert.False(await manager.CheckPasswordAsync(user, "password"));
    }

    /// <summary>
    /// Создаёт пользователя с уникальными допустимыми именем и email,
    /// чтобы парольные тесты не зависели от совпадения учётных данных.
    /// </summary>
    private static ApplicationUser NewUser()
    {
        var userName = Guid.NewGuid().ToString("N");
        return new ApplicationUser { UserName = userName, Email = userName + "@example.com" };
    }
}
