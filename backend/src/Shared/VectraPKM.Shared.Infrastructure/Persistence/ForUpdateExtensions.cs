using Microsoft.EntityFrameworkCore;

namespace VectraPKM.Shared.Infrastructure.Persistence;

/// <summary>Явная блокировка строк PostgreSQL в запросах EF Core.</summary>
public static class ForUpdateExtensions
{
    internal const string QueryTag = "VectraPKM:ForUpdate";

    /// <summary>
    /// Помечает простой SELECT сущностей для блокировки строк до завершения транзакции.
    /// Требует регистрации ForUpdateInterceptor и явной транзакции PostgreSQL.
    /// Не предназначен для агрегатов, DISTINCT, GROUP BY, объединений и split queries.
    /// </summary>
    /// <typeparam name="T">Тип сущности.</typeparam>
    /// <param name="query">Запрос строк, которые нужно заблокировать.</param>
    /// <returns>Запрос с меткой для добавления FOR UPDATE при выполнении.</returns>
    public static IQueryable<T> ForUpdate<T>(this IQueryable<T> query) where T : class
        => query.TagWith(QueryTag);
}
