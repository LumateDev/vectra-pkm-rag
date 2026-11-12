using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using VectraPKM.Auth.Application.Abstractions;
using VectraPKM.Auth.Application.Contracts;

namespace VectraPKM.Auth.Infrastructure.Authentication;

/// <summary>Выпускает JWT с подписью HS256 без персональных данных пользователя.</summary>
internal sealed class JwtAccessTokenIssuer(JwtOptions options, SigningCredentials credentials) : IAccessTokenIssuer
{
    public IssuedAccessToken Issue(Guid userId, Guid sessionId)
    {
        // JWT использует целые секунды; срок в ответе точно совпадает с exp.
        var now = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        var expiresAt = now.AddMinutes(options.AccessTokenLifetimeMinutes);
        var token = new JwtSecurityToken(options.Issuer, options.Audience,
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Sid, sessionId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            ],
            notBefore: now.UtcDateTime, expires: expiresAt.UtcDateTime, signingCredentials: credentials);

        return new IssuedAccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
