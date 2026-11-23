using Microsoft.EntityFrameworkCore;
using VectraPKM.Projects.Application;
using VectraPKM.Projects.Domain.Entities;

namespace VectraPKM.Projects.Infrastructure.Persistence;

internal sealed class NoteStore(ProjectsDbContext db) : INoteStore
{
    public async Task AddAsync(Note note, CancellationToken cancellationToken)
    {
        db.Notes.Add(note);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NoteSummary>> ListAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return await db.Notes.AsNoTracking()
            .Where(note => note.ProjectId == projectId)
            .OrderByDescending(note => note.UpdatedAt)
            .ThenBy(note => note.Id)
            .Select(note => new NoteSummary(note.Id, note.Title, note.CreatedAt, note.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public Task<Note?> FindAsync(Guid projectId, Guid noteId, CancellationToken cancellationToken)
        => db.Notes.AsNoTracking().SingleOrDefaultAsync(
            note => note.Id == noteId && note.ProjectId == projectId,
            cancellationToken);

    public async Task<bool> SaveContentAsync(Note note, CancellationToken cancellationToken)
    {
        var updated = await db.Notes
            .Where(item => item.Id == note.Id && item.ProjectId == note.ProjectId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.Title, note.Title)
                .SetProperty(item => item.Markdown, note.Markdown)
                .SetProperty(item => item.UpdatedAt, note.UpdatedAt),
                cancellationToken);
        return updated == 1;
    }
}
