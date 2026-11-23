using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VectraPKM.Projects.Domain.Entities;

namespace VectraPKM.Projects.Infrastructure.Persistence.Configurations;

/// <summary>Настройки хранения заметок и их связи с проектами.</summary>
internal sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("Notes");
        builder.HasKey(note => note.Id);
        builder.Property(note => note.Id).ValueGeneratedNever();
        builder.Property(note => note.Title).IsRequired();
        builder.Property(note => note.Markdown).IsRequired();

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(note => note.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
