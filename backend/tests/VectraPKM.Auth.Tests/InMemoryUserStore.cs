using Microsoft.AspNetCore.Identity;
using VectraPKM.Auth.Infrastructure.Identity;

namespace VectraPKM.Auth.Tests;

// Minimal test double for UserManager password scenarios. Not a production persistence implementation.
internal sealed class InMemoryUserStore : IUserPasswordStore<ApplicationUser>, IUserEmailStore<ApplicationUser>
{
    internal Dictionary<Guid, ApplicationUser> Users { get; } = [];
    internal int UpdateCount { get; private set; }

    public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        Users.Add(user.Id, user);
        return Task.FromResult(IdentityResult.Success);
    }
    public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        Users[user.Id] = user;
        UpdateCount++;
        return Task.FromResult(IdentityResult.Success);
    }
    public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        Users.Remove(user.Id);
        return Task.FromResult(IdentityResult.Success);
    }
    public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        => Task.FromResult(Users.GetValueOrDefault(Guid.Parse(userId)));
    public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        => Task.FromResult(Users.Values.FirstOrDefault(user => user.NormalizedUserName == normalizedUserName));
    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.Id.ToString());
    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.UserName);
    public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }
    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.NormalizedUserName);
    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }
    public Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }
    public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.PasswordHash);
    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.PasswordHash is not null);
    public Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.CompletedTask;
    }
    public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.Email);
    public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.EmailConfirmed);
    public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }
    public Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        => Task.FromResult(Users.Values.FirstOrDefault(user => user.NormalizedEmail == normalizedEmail));
    public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.NormalizedEmail);
    public Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }
    public void Dispose() { }
}
