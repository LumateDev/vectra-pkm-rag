using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using VectraPKM.Auth.Api.Contracts;
using VectraPKM.Auth.Api.Controllers;
using VectraPKM.Auth.Application;
using VectraPKM.Auth.Application.Abstractions;
using VectraPKM.Auth.Infrastructure.Identity;
using VectraPKM.Auth.Infrastructure.Persistence;
using VectraPKM.Shared.Api.Contracts;
using Xunit;

namespace VectraPKM.Auth.Tests;

/// <summary>Проверки входа через контроллер, прикладной сервис и настоящий UserManager.</summary>
public sealed class AuthenticationTests
{
    /// <summary>Вход возвращает JWT и защищённую refresh-cookie; в БД сохраняется только хеш refresh с тем же sid.</summary>
    /// <param name="login">Email или имя существующего пользователя в другом регистре.</param>
    [Theory(DisplayName = "Авторизация: верный пароль возвращает JWT")]
    [InlineData("ALEX@example.com")]
    [InlineData("aLEX")]
    public async Task Auth_returns_signed_access_token(string login)
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var controller = await CreateRegisteredController(scope.ServiceProvider);

        var response = await controller.Auth(new AuthRequest { Login = login, Password = " password " }, default);

        var result = Assert.IsType<OkObjectResult>(response.Result);
        var envelope = Assert.IsType<ApiResult<AuthResponse>>(result.Value);
        Assert.Null(envelope.Error);
        var data = Assert.IsType<AuthResponse>(envelope.Data);
        var parameters = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme).TokenValidationParameters;
        var principal = new JwtSecurityTokenHandler { MapInboundClaims = false }
            .ValidateToken(data.AccessToken, parameters, out var validatedToken);
        var jwt = Assert.IsType<JwtSecurityToken>(validatedToken);
        Assert.Equal(SecurityAlgorithms.HmacSha256, jwt.Header.Alg);
        Assert.Equal(data.User.Id.ToString(), principal.FindFirst("sub")?.Value);
        Assert.True(Guid.TryParse(principal.FindFirst("sid")?.Value, out var sessionId));
        Assert.NotEqual(Guid.Empty, sessionId);
        Assert.Equal("VectraPKM.Tests", jwt.Issuer);
        Assert.Equal("VectraPKM.Tests.Api", Assert.Single(jwt.Audiences));
        Assert.Equal("alex@example.com", data.User.Email);
        Assert.Equal("Alex", data.User.UserName);
        Assert.DoesNotContain(jwt.Claims, claim => claim.Type is "email" or "userName" or "username");
        Assert.False(string.IsNullOrWhiteSpace(jwt.Id));
        Assert.Equal(data.ExpiresAt.UtcDateTime, jwt.ValidTo);
        Assert.Equal(TimeSpan.FromMinutes(15), jwt.ValidTo - jwt.ValidFrom);
        Assert.Equal("no-store", controller.Response.Headers.CacheControl.ToString());
        var cookie = SetCookieHeaderValue.Parse(controller.Response.Headers.SetCookie.ToString());
        Assert.Equal("refreshToken", cookie.Name.ToString());
        Assert.True(cookie.HttpOnly);
        Assert.True(cookie.Secure);
        Assert.Equal(Microsoft.Net.Http.Headers.SameSiteMode.Strict, cookie.SameSite);
        Assert.Equal("/auth", cookie.Path.ToString());
        var rawRefresh = cookie.Value.ToString();
        Assert.Equal(64, Base64UrlEncoder.DecodeBytes(rawRefresh).Length);
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var stored = Assert.Single(await context.RefreshTokens.AsNoTracking().ToListAsync());
        Assert.Equal(sessionId, stored.SessionId);
        Assert.Equal(data.User.Id, stored.UserId);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawRefresh))), stored.TokenHash);
        Assert.NotEqual(rawRefresh, stored.TokenHash);
        Assert.Equal(TimeSpan.FromDays(30), stored.ExpiresAt - stored.CreatedAt);
    }

    /// <summary>Два успешных входа одного пользователя создают разные sid и разные идентификаторы access-токенов.</summary>
    [Fact(DisplayName = "Авторизация: каждый вход получает отдельный sid")]
    public async Task Separate_logins_have_distinct_session_ids()
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var controller = await CreateRegisteredController(scope.ServiceProvider);
        var request = new AuthRequest { Login = "Alex", Password = " password " };

        var first = Assert.IsType<OkObjectResult>((await controller.Auth(request, default)).Result);
        var second = Assert.IsType<OkObjectResult>((await controller.Auth(request, default)).Result);
        var handler = new JwtSecurityTokenHandler();
        var firstToken = handler.ReadJwtToken(Assert.IsType<ApiResult<AuthResponse>>(first.Value).Data!.AccessToken);
        var secondToken = handler.ReadJwtToken(Assert.IsType<ApiResult<AuthResponse>>(second.Value).Data!.AccessToken);

        Assert.Equal(firstToken.Subject, secondToken.Subject);
        Assert.NotEqual(firstToken.Claims.Single(claim => claim.Type == "sid").Value,
            secondToken.Claims.Single(claim => claim.Type == "sid").Value);
        Assert.NotEqual(firstToken.Id, secondToken.Id);
    }

    /// <summary>Проверка JWT отклоняет неверную подпись, алгоритм, издателя, получателя и истёкший срок.</summary>
    /// <param name="failure">Изменяемая часть параметров проверки или времени.</param>
    [Theory(DisplayName = "Авторизация: JWT отклоняется при неверных параметрах или истечении срока")]
    [InlineData("signature")]
    [InlineData("issuer")]
    [InlineData("audience")]
    [InlineData("expiration")]
    [InlineData("algorithm")]
    public async Task Invalid_jwt_is_rejected(string failure)
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var controller = await CreateRegisteredController(scope.ServiceProvider);
        var response = await controller.Auth(new AuthRequest { Login = "Alex", Password = " password " }, default);
        var result = Assert.IsType<OkObjectResult>(response.Result);
        var data = Assert.IsType<ApiResult<AuthResponse>>(result.Value).Data!;
        var parameters = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme).TokenValidationParameters.Clone();
        var token = data.AccessToken;
        switch (failure)
        {
            case "signature": parameters.IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("Another-Test-Signing-Key-That-Is-At-Least-Thirty-Two-Bytes-Long-2026")); break;
            case "issuer": parameters.ValidIssuer = "other-issuer"; break;
            case "audience": parameters.ValidAudience = "other-audience"; break;
            case "expiration":
            case "algorithm":
                // Подписываем тестовым ключом истёкший токен или токен с запрещённым алгоритмом.
                {
                    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthTestServices.JwtKey));
                    var jwt = new JwtSecurityToken("VectraPKM.Tests", "VectraPKM.Tests.Api",
                        notBefore: DateTime.UtcNow.AddDays(-2),
                        expires: failure == "expiration" ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddMinutes(15),
                        signingCredentials: new SigningCredentials(signingKey,
                            failure == "algorithm" ? SecurityAlgorithms.HmacSha512 : SecurityAlgorithms.HmacSha256));
                    token = new JwtSecurityTokenHandler().WriteToken(jwt);
                }
                break;
        }

        Assert.ThrowsAny<SecurityTokenException>(() => new JwtSecurityTokenHandler()
            .ValidateToken(token, parameters, out _));
    }

    /// <summary>Выпуск JWT использует срок из настроек вместо жёстко заданных 15 минут.</summary>
    [Fact(DisplayName = "Авторизация: срок JWT берётся из конфигурации")]
    public async Task Access_token_lifetime_uses_configuration()
    {
        using var services = AuthTestServices.Create(accessTokenLifetimeMinutes: 7);
        using var scope = services.CreateScope();
        var controller = await CreateRegisteredController(scope.ServiceProvider);

        var response = await controller.Auth(new AuthRequest { Login = "Alex", Password = " password " }, default);

        var result = Assert.IsType<OkObjectResult>(response.Result);
        var data = Assert.IsType<ApiResult<AuthResponse>>(result.Value).Data!;
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(data.AccessToken);
        Assert.Equal(TimeSpan.FromMinutes(7), jwt.ValidTo - jwt.ValidFrom);
        Assert.Equal(data.ExpiresAt.UtcDateTime, jwt.ValidTo);
    }

    /// <summary>Неизвестный email или username и неверный пароль дают одинаковый 401 без данных и токена.</summary>
    /// <param name="login">Существующий или неизвестный email либо username.</param>
    /// <param name="password">Неверный пароль, в том числе с удалёнными значимыми пробелами.</param>
    [Theory(DisplayName = "Авторизация: неверные учётные данные возвращают одинаковый 401")]
    [InlineData("unknown@example.com", " password ")]
    [InlineData("UnknownUser", " password ")]
    [InlineData("alex@example.com", "incorrect")]
    [InlineData("Alex", "incorrect")]
    [InlineData("alex@example.com", "password")]
    public async Task Invalid_credentials_return_unauthorized(string login, string password)
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var controller = await CreateRegisteredController(scope.ServiceProvider);

        AssertUnauthorized(await controller.Auth(new AuthRequest { Login = login, Password = password }, default));
    }

    /// <summary>Пять неверных паролей блокируют вход даже с верным паролем; после окончания блокировки вход разрешён.</summary>
    [Fact(DisplayName = "Авторизация: пять ошибок блокируют вход на время")]
    public async Task Failed_attempts_lock_user_until_expiration()
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var controller = await CreateRegisteredController(scope.ServiceProvider);
        for (var attempt = 0; attempt < 5; attempt++)
        {
            AssertUnauthorized(await controller.Auth(new AuthRequest { Login = attempt % 2 == 0 ? "Alex" : "alex@example.com", Password = "incorrect" }, default));
        }

        var request = new AuthRequest { Login = "alex@example.com", Password = " password " };
        AssertUnauthorized(await controller.Auth(request, default));
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var stored = await manager.FindByEmailAsync("alex@example.com");
        Assert.NotNull(stored);
        Assert.True(await manager.IsLockedOutAsync(stored));
        Assert.True((await manager.SetLockoutEndDateAsync(stored, DateTimeOffset.UtcNow.AddSeconds(-1))).Succeeded);

        Assert.IsType<OkObjectResult>((await controller.Auth(request, default)).Result);
    }

    /// <summary>Успешный вход обнуляет счётчик предыдущих неудачных попыток.</summary>
    [Fact(DisplayName = "Авторизация: успешный вход сбрасывает счётчик ошибок")]
    public async Task Successful_auth_resets_failed_attempts()
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var controller = await CreateRegisteredController(scope.ServiceProvider);
        AssertUnauthorized(await controller.Auth(new AuthRequest { Login = "alex@example.com", Password = "incorrect" }, default));
        var stored = Assert.Single(await AuthTestServices.ReadUsersAsync(services));
        Assert.Equal(1, stored.AccessFailedCount);

        Assert.IsType<OkObjectResult>((await controller.Auth(
            new AuthRequest { Login = "Alex", Password = " password " }, default)).Result);

        Assert.Equal(0, Assert.Single(await AuthTestServices.ReadUsersAsync(services)).AccessFailedCount);
    }

    /// <summary>Заранее отменённый запрос прекращает вход и не увеличивает счётчик ошибок.</summary>
    [Fact(DisplayName = "Авторизация: отменённый запрос не проверяет пароль")]
    public async Task Cancelled_auth_does_not_increment_failed_attempts()
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var controller = await CreateRegisteredController(scope.ServiceProvider);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => controller.Auth(
            new AuthRequest { Login = "alex@example.com", Password = "incorrect" }, cancellation.Token));

        Assert.Equal(0, Assert.Single(await AuthTestServices.ReadUsersAsync(services)).AccessFailedCount);
    }

    /// <summary>Создаёт контроллер с настоящими сервисами и регистрирует пользователя для проверки входа.</summary>
    private static async Task<AuthController> CreateRegisteredController(IServiceProvider services)
    {
        var auth = new AuthService(services.GetRequiredService<IUserRegistration>(), services.GetRequiredService<IUserAuthentication>(), services.GetRequiredService<IAccessTokenIssuer>(), services.GetRequiredService<IRefreshTokenService>());
        var controller = new AuthController(auth)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        await controller.Register(new RegisterRequest { Email = "alex@example.com", UserName = "Alex", Password = " password " }, default);
        return controller;
    }

    /// <summary>Проверяет отказ во входе с общим сообщением и отсутствием успешных данных.</summary>
    private static void AssertUnauthorized(ActionResult<ApiResult<AuthResponse>> response)
    {
        var result = Assert.IsType<UnauthorizedObjectResult>(response.Result);
        var envelope = Assert.IsType<ApiResult<AuthResponse>>(result.Value);
        Assert.Null(envelope.Data);
        Assert.Equal("Неверный логин или пароль.", envelope.Error);
    }
}
