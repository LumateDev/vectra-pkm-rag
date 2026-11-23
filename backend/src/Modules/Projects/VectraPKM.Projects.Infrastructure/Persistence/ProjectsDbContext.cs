using Microsoft.EntityFrameworkCore;
using VectraPKM.Projects.Domain.Entities;
using VectraPKM.Projects.Infrastructure.Persistence.Configurations;

namespace VectraPKM.Projects.Infrastructure.Persistence;

/// <summary>Контекст данных модуля личных проектов.</summary>
public sealed class ProjectsDbContext(DbContextOptions<ProjectsDbContext> options) : DbContext(options)
{
    /// <summary>Личные проекты пользователей.</summary>
    public DbSet<Project> Projects => Set<Project>();

    /// <summary>Markdown-заметки в личных проектах.</summary>
    public DbSet<Note> Notes => Set<Note>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        var owner = builder.Entity<ProjectOwner>();
        owner.ToTable("AspNetUsers", table => table.ExcludeFromMigrations());
        owner.HasKey(user => user.Id);

        builder.ApplyConfiguration(new ProjectConfiguration());
        builder.ApplyConfiguration(new NoteConfiguration());
    }
}
