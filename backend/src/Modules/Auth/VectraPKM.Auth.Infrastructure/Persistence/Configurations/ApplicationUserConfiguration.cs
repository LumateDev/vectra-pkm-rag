using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VectraPKM.Auth.Infrastructure.Identity;

namespace VectraPKM.Auth.Infrastructure.Persistence.Configurations;

/// <summary>
/// Дополнительные настройки хранения пользователя поверх стандартной модели Identity.
/// </summary>
internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasIndex(user => user.NormalizedEmail).IsUnique();
    }
}
