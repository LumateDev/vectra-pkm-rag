using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using VectraPKM.Auth.Api.Contracts;
using VectraPKM.Auth.Api.Controllers;
using VectraPKM.Auth.Application;
using VectraPKM.Auth.Application.Abstractions;
using VectraPKM.Auth.Infrastructure.Identity;
using VectraPKM.Shared.Api.Contracts;
using Xunit;

namespace VectraPKM.Auth.Tests;

/// <summary>Проверки входа через контроллер, прикладной сервис и настоящий UserManager.</summary>
public sealed class AuthenticationTests
{
    /// <summary>Вход по email или username без учёта регистра сохраняет пробелы пароля и возвращает 200 с заглушкой токена.</summary>
    /// <param name="login">Email или имя существующего пользователя в другом регистре.</param>
    [Theory(DisplayName = "Авторизация: верный пароль возвращает временный accessToken")]
    [InlineData("ALEX@example.com")]
    [InlineData("aLEX")]
    public async Task Auth_returns_temporary_access_token(string login)
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var controller = await CreateRegisteredController(scope.ServiceProvider);

        var response = await controller.Auth(new AuthRequest { Login = login, Password = " password " }, default);

        var result = Assert.IsType<OkObjectResult>(response.Result);
        var envelope = Assert.IsType<ApiResult<AuthResponse>>(result.Value);
        Assert.Null(envelope.Error);
        var data = Assert.IsType<AuthResponse>(envelope.Data);
        Assert.Equal("temptoken", data.AccessToken);
        Assert.Equal("no-store", controller.Response.Headers.CacheControl.ToString());
        Assert.False(controller.Response.Headers.ContainsKey("Set-Cookie"));
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
        var auth = new AuthService(services.GetRequiredService<IUserRegistration>(), services.GetRequiredService<IUserAuthentication>());
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
