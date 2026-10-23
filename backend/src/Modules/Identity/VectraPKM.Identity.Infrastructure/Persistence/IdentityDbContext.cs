using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VectraPKM.Identity.Domain.Entities;
using VectraPKM.Identity.Infrastructure.Identity;
using VectraPKM.Identity.Infrastructure.Persistence.Configurations;

namespace VectraPKM.Identity.Infrastructure.Persistence;

/// <summary>
/// Контекст данных модуля Identity: пользователи и refresh-токены, без ролей.
/// </summary>
public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
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
