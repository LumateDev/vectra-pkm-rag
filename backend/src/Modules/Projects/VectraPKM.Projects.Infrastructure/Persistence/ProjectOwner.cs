namespace VectraPKM.Projects.Infrastructure.Persistence;

/// <summary>Ключ учётной записи из таблицы Auth, на которую ссылается проект.</summary>
internal sealed class ProjectOwner
{
    public Guid Id { get; private set; }
}
