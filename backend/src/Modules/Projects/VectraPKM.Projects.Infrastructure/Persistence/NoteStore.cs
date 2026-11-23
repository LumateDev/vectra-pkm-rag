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
}
