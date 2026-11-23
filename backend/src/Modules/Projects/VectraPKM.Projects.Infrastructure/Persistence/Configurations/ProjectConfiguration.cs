using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VectraPKM.Projects.Domain.Entities;

namespace VectraPKM.Projects.Infrastructure.Persistence.Configurations;

/// <summary>Настройки хранения проектов и их связи с владельцами.</summary>
internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(project => project.Id);
        builder.Property(project => project.Id).ValueGeneratedNever();
        builder.Property(project => project.Name).IsRequired();

        builder.HasOne<ProjectOwner>()
            .WithMany()
            .HasForeignKey(project => project.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
