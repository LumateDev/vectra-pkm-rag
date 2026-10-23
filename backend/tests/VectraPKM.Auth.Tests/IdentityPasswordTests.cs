using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VectraPKM.Auth.Infrastructure.Identity;
using Xunit;

namespace VectraPKM.Auth.Tests;

public sealed class IdentityPasswordTests
{
    [Fact]
    public async Task UserManager_stores_only_hash_and_rejects_incorrect_password()
    {
        using var services = PasswordTestServices.Create();
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

    [Theory]
    [InlineData(8)]
    [InlineData(128)]
    public async Task Passwords_at_both_length_boundaries_are_accepted(int length)
    {
        using var services = PasswordTestServices.Create();
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = NewUser();
        var password = new string('a', length);

        Assert.True((await manager.CreateAsync(user, password)).Succeeded);
        Assert.True(await manager.CheckPasswordAsync(user, password));
    }

    [Theory]
    [InlineData(0, "PasswordTooShort")]
    [InlineData(7, "PasswordTooShort")]
    [InlineData(129, "PasswordTooLong")]
    [InlineData(1024, "PasswordTooLong")]
    public async Task Invalid_lengths_are_rejected_before_hashing_or_storing(int length, string expectedCode)
    {
        using var services = PasswordTestServices.Create();
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var store = services.GetRequiredService<InMemoryUserStore>();
        var user = NewUser();

        var result = await manager.CreateAsync(user, new string('a', length));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == expectedCode);
        Assert.Null(user.PasswordHash);
        Assert.Empty(store.Users);
    }

    [Theory]
    [InlineData("aaaaaaaa")]
    [InlineData("AAAAAAAA")]
    [InlineData("12345678")]
    [InlineData("!!!!!!!!")]
    [InlineData(" пароль ")]
    public async Task Character_composition_rules_are_not_required(string password)
    {
        using var services = PasswordTestServices.Create();
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = NewUser();

        Assert.True((await manager.CreateAsync(user, password)).Succeeded);
        Assert.True(await manager.CheckPasswordAsync(user, password));
    }

    [Fact]
    public async Task Identity_rehashes_an_old_password_hash_after_successful_verification()
    {
        using var services = PasswordTestServices.Create();
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
        var storedUser = await manager.FindByIdAsync(user.Id.ToString());
        Assert.Equal(user.PasswordHash, storedUser!.PasswordHash);
        Assert.True(services.GetRequiredService<InMemoryUserStore>().UpdateCount > 0);
    }

    [Fact]
    public void Identity_uses_the_builtin_password_hasher()
    {
        using var services = PasswordTestServices.Create();
        using var scope = services.CreateScope();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();

        Assert.IsType<PasswordHasher<ApplicationUser>>(hasher);
    }

    [Fact]
    public async Task Same_password_produces_distinct_hashes_that_both_verify()
    {
        using var services = PasswordTestServices.Create();
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

    [Theory]
    [InlineData("Password", "password")]
    [InlineData("пароль🙂", "пароль🙃")]
    public async Task Verification_preserves_case_and_unicode(string password, string differentPassword)
    {
        using var services = PasswordTestServices.Create();
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = NewUser();

        Assert.True((await manager.CreateAsync(user, password)).Succeeded);
        Assert.True(await manager.CheckPasswordAsync(user, password));
        Assert.False(await manager.CheckPasswordAsync(user, differentPassword));
    }

    [Fact]
    public async Task Tampered_hash_is_rejected()
    {
        using var services = PasswordTestServices.Create();
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = NewUser();
        Assert.True((await manager.CreateAsync(user, "password")).Succeeded);
        var bytes = Convert.FromBase64String(user.PasswordHash!);
        bytes[^1] ^= 1;
        user.PasswordHash = Convert.ToBase64String(bytes);

        Assert.False(await manager.CheckPasswordAsync(user, "password"));
    }

    private static ApplicationUser NewUser()
    {
        var email = Guid.NewGuid().ToString("N") + "@example.com";
        return new ApplicationUser { UserName = email, Email = email };
    }
}
