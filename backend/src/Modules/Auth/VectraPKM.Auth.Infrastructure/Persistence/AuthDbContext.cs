using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VectraPKM.Auth.Domain.Entities;
using VectraPKM.Auth.Infrastructure.Identity;
using VectraPKM.Auth.Infrastructure.Persistence.Configurations;

namespace VectraPKM.Auth.Infrastructure.Persistence;

/// <summary>
/// Контекст данных модуля Auth: пользователи и refresh-токены, без ролей.
/// </summary>
public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options)
    : IdentityUserContext<ApplicationUser, Guid>(options)
{
    /// <summary>
    /// Refresh-токены, включая использованные записи для обнаружения повторного использования.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>
    /// Применяет стандартную модель Identity и настройки таблиц модуля.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new ApplicationUserConfiguration());
        builder.ApplyConfiguration(new RefreshTokenConfiguration());
    }
}
