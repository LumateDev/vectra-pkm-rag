using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VectraPKM.Identity.Infrastructure.Identity;

namespace VectraPKM.Identity.Infrastructure.Persistence.Configurations;

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
