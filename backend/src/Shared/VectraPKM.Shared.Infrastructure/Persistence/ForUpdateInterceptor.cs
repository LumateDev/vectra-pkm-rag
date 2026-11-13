using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace VectraPKM.Shared.Infrastructure.Persistence;

/// <summary>Добавляет FOR UPDATE к помеченным SELECT-запросам PostgreSQL; поддерживает sync и async.</summary>
public sealed class ForUpdateInterceptor : DbCommandInterceptor
{
    /// <inheritdoc />
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        ApplyLock(command, eventData);
        return result;
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        ApplyLock(command, eventData);
        return ValueTask.FromResult(result);
    }

    private static void ApplyLock(DbCommand command, CommandEventData eventData)
    {
        if (!HasLockTag(command.CommandText)) return;

        if (eventData.Context?.Database.ProviderName != "Npgsql.EntityFrameworkCore.PostgreSQL")
            throw new NotSupportedException("ForUpdate поддерживает только PostgreSQL.");
        if (command.Transaction is null)
            throw new InvalidOperationException("ForUpdate требует явной транзакции до выполнения запроса.");

        var sql = command.CommandText.TrimEnd(); // убрать пробелы и переносы в конце
        sql = sql.TrimEnd(';');                  // убрать завершающий ;
        sql = sql.TrimEnd();                     // убрать пробелы перед ним

        if (!sql.EndsWith("\nFOR UPDATE", StringComparison.Ordinal))
            command.CommandText = sql + "\nFOR UPDATE";
    }

    private static bool HasLockTag(string sql)
    {
        // Проверяем только начальные комментарии EF, а не текст внутри SQL-литералов.
        using var reader = new StringReader(sql);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var text = line.Trim();
            if (text == "-- " + ForUpdateExtensions.QueryTag) return true;
            if (text.Length != 0 && !text.StartsWith("--", StringComparison.Ordinal)) break;
        }
        return false;
    }
}
