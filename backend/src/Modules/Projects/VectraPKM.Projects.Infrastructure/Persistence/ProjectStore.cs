using Microsoft.EntityFrameworkCore;
using VectraPKM.Projects.Application;
using VectraPKM.Projects.Domain.Entities;

namespace VectraPKM.Projects.Infrastructure.Persistence;

internal sealed class ProjectStore(ProjectsDbContext db) : IProjectStore
{
    public async Task AddAsync(Project project, CancellationToken cancellationToken)
    {
        db.Projects.Add(project);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> ListByOwnerAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        return await db.Projects.AsNoTracking()
            .Where(project => project.OwnerId == ownerId)
            .OrderByDescending(project => project.CreatedAt)
            .ThenBy(project => project.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Project?> FindByOwnerAsync(Guid id, Guid ownerId, CancellationToken cancellationToken)
        => db.Projects.AsNoTracking().SingleOrDefaultAsync(
            project => project.Id == id && project.OwnerId == ownerId,
            cancellationToken);
}
