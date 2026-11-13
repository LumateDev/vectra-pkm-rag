using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using VectraPKM.Auth.Application.Abstractions;
using VectraPKM.Auth.Application.Contracts;
using VectraPKM.Auth.Application.Exceptions;
using VectraPKM.Auth.Domain.Entities;
using VectraPKM.Auth.Infrastructure.Persistence;
using VectraPKM.Shared.Infrastructure.Persistence;

namespace VectraPKM.Auth.Infrastructure.Authentication;

/// <summary>Хранит только хеши refresh-токенов; ротация сериализуется блокировкой пользователя PostgreSQL.</summary>
internal sealed class RefreshTokenService(AuthDbContext db, JwtOptions options, ILogger<RefreshTokenService> logger) : IRefreshTokenService
{
    public async Task<IssuedRefreshToken> CreateAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var issued = Create(userId, sessionId, now, now.AddDays(options.RefreshTokenLifetimeDays));
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Refresh family created. UserId={UserId}, SessionId={SessionId}", userId, sessionId);
        return issued;
    }

    public async Task<RefreshedSession> RotateAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var original = await FindAsync(refreshToken, cancellationToken)
            ?? throw new AuthInvalidRefreshTokenException();
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        // Строка пользователя существует всё время жизни семейства. Общая блокировка
        // согласует refresh/logout между процессами без новой таблицы сессий.
        var user = await db.Users.Where(item => item.Id == original.UserId)
            .AsNoTracking().ForUpdate().SingleOrDefaultAsync(cancellationToken) 
            ?? throw new AuthInvalidRefreshTokenException();

        // После блокировки читаем актуальное состояние без использования Change Tracker.
        var token = await db.RefreshTokens.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == original.Id, cancellationToken)
            ?? throw new AuthInvalidRefreshTokenException();
        var now = DateTimeOffset.UtcNow;
        
        if (token.RefreshedAt is not null)
        {
            await db.RefreshTokens.Where(item => item.SessionId == token.SessionId && item.RevokedAt == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.RevokedAt, now), cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            logger.LogWarning("Refresh reuse detected; family revoked. UserId={UserId}, SessionId={SessionId}", token.UserId, token.SessionId);
            // Отзыв уже сохранён: исключение не должно откатывать обнаруженный reuse.
            throw new AuthInvalidRefreshTokenException();
        }

        // 	Токен уже отозван или истёк
        if (token.RevokedAt is not null || token.ExpiresAt <= now)
            throw new AuthInvalidRefreshTokenException();

        token.MarkRefreshed(now);
        // Сущность прочитана без tracking: сохраняем отметку явно внутри текущей транзакции.
        await db.RefreshTokens.Where(item => item.Id == token.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.RefreshedAt, token.RefreshedAt), cancellationToken);
        var issued = Create(token.UserId, token.SessionId, now, token.ExpiresAt);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Refresh rotated. UserId={UserId}, SessionId={SessionId}", token.UserId, token.SessionId);
        return new RefreshedSession(new AuthenticationResult(user.Id, user.Email!, user.UserName!), token.SessionId, issued);
    }

    public async Task RevokeAsync(string? refreshToken, CancellationToken cancellationToken)
    {
        var token = await FindAsync(refreshToken, cancellationToken);
        if (token is null) return;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var user = await db.Users.Where(item => item.Id == token.UserId)
            .AsNoTracking().ForUpdate().SingleOrDefaultAsync(cancellationToken);
        if (user is null) return;

        var now = DateTimeOffset.UtcNow;
        await db.RefreshTokens.Where(item => item.SessionId == token.SessionId && item.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.RevokedAt, now), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Refresh family revoked. UserId={UserId}, SessionId={SessionId}", token.UserId, token.SessionId);
    }

    private IssuedRefreshToken Create(Guid userId, Guid sessionId, DateTimeOffset now, DateTimeOffset expiresAt)
    {
        var value = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
        db.RefreshTokens.Add(new RefreshToken(userId, sessionId, Hash(value), now, expiresAt));
        return new IssuedRefreshToken(value, expiresAt);
    }

    private Task<RefreshTokenReference?> FindAsync(string? value, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 86)
            return Task.FromResult<RefreshTokenReference?>(null);
        var hash = Hash(value);
        return db.RefreshTokens.Where(item => item.TokenHash == hash)
            .Select(item => new RefreshTokenReference(item.Id, item.UserId, item.SessionId))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private sealed record RefreshTokenReference(Guid Id, Guid UserId, Guid SessionId);

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
