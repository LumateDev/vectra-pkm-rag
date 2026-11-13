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

/// <summary>
/// Базовые проверки цепочки регистрации: контроллер, AuthService и Identity.
/// Контроллер вызывается напрямую; EF-хранилище использует провайдер InMemory вместо PostgreSQL.
/// </summary>
public sealed class RegistrationTests
{
    /// <summary>
    /// Корректный запрос создаёт пользователя с хешем пароля и возвращает 201 с открытыми данными.
    /// Токены и cookie при регистрации не выдаются.
    /// </summary>
    [Fact(DisplayName = "Регистрация: создаёт пользователя и возвращает 201 без cookie")]
    public async Task Register_creates_user_and_returns_public_data()
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var controller = CreateController(scope.ServiceProvider);
        var request = new RegisterRequest { Email = "alex@example.com", UserName = "Alex", Password = " password " };

        var response = await controller.Register(request, default);

        var result = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        var envelope = Assert.IsType<ApiResult<RegisterResponse>>(result.Value);
        Assert.Null(envelope.Error);
        var user = Assert.IsType<RegisterResponse>(envelope.Data);
        Assert.NotEqual(Guid.Empty, user.UserId);
        Assert.Equal(request.Email, user.Email);
        Assert.Equal(request.UserName, user.UserName);
        var stored = Assert.Single(await AuthTestServices.ReadUsersAsync(services));
        Assert.Equal(user.UserId, stored.Id);
        Assert.NotNull(stored.PasswordHash);
        Assert.NotEqual(request.Password, stored.PasswordHash);
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.True(await manager.CheckPasswordAsync(stored, request.Password));
        Assert.False(controller.Response.Headers.ContainsKey("Set-Cookie"));
    }

    /// <summary>
    /// Повторный email с другим регистром и свободным username возвращает 409 и ApiResult с сообщением.
    /// В хранилище остаётся только первый пользователь.
    /// </summary>
    [Fact(DisplayName = "Регистрация: повторный email возвращает 409 и сообщение")]
    public async Task Duplicate_email_returns_conflict()
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var controller = CreateController(scope.ServiceProvider);
        await controller.Register(new RegisterRequest { Email = "alex@example.com", UserName = "Alex", Password = "password" }, default);

        var response = await controller.Register(new RegisterRequest { Email = "ALEX@example.com", UserName = "AnotherUser", Password = "password" }, default);

        var result = AssertError(response, StatusCodes.Status409Conflict);
        Assert.Equal("Этот email уже используется.", result.Error);
        Assert.Single(await AuthTestServices.ReadUsersAsync(services));
    }

    /// <summary>
    /// Занятый username с другим регистром и новым email возвращает 409 и ApiResult с сообщением.
    /// В хранилище остаётся только первый пользователь.
    /// </summary>
    [Fact(DisplayName = "Регистрация: повторный username возвращает 409 и сообщение")]
    public async Task Duplicate_username_returns_conflict()
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var controller = CreateController(scope.ServiceProvider);
        await controller.Register(new RegisterRequest { Email = "alex@example.com", UserName = "Alex", Password = "password" }, default);

        var response = await controller.Register(new RegisterRequest { Email = "another@example.com", UserName = "alex", Password = "password" }, default);

        var result = AssertError(response, StatusCodes.Status409Conflict);
        Assert.Equal("Это имя пользователя уже используется.", result.Error);
        Assert.Single(await AuthTestServices.ReadUsersAsync(services));
    }

    /// <summary>
    /// Ошибки email, username и пароля, обнаруженные Identity, возвращаются контроллером как 400 с сообщением.
    /// Создание пользователя при этом не происходит. Автоматическая MVC-валидация здесь не вызывается.
    /// </summary>
    /// <param name="email">Проверяемый email.</param>
    /// <param name="userName">Проверяемое имя пользователя.</param>
    /// <param name="password">Проверяемый пароль.</param>
    [Theory(DisplayName = "Регистрация: неверные данные возвращают 400 и сообщение")]
    [InlineData("invalid-email", "Alex", "password")]
    [InlineData("alex@example.com", "ab", "password")]
    [InlineData("alex@example.com", "Alex", "short")]
    public async Task Invalid_fields_return_validation_error(string email, string userName, string password)
    {
        using var services = AuthTestServices.Create();
        using var scope = services.CreateScope();
        var controller = CreateController(scope.ServiceProvider);

        var response = await controller.Register(new RegisterRequest { Email = email, UserName = userName, Password = password }, default);

        AssertError(response, StatusCodes.Status400BadRequest);
        Assert.Empty(await AuthTestServices.ReadUsersAsync(services));
    }

    /// <summary>Создаёт настоящий контроллер и сервис с инфраструктурной реализацией регистрации.</summary>
    private static AuthController CreateController(IServiceProvider services)
    {
        var auth = new AuthService(services.GetRequiredService<IUserRegistration>(),
            services.GetRequiredService<IUserAuthentication>(), services.GetRequiredService<IAccessTokenIssuer>(), services.GetRequiredService<IRefreshTokenService>());
        return new AuthController(auth)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    /// <summary>Проверяет статус и ApiResult с сообщением без успешных данных.</summary>
    private static ApiResult<RegisterResponse> AssertError(ActionResult<ApiResult<RegisterResponse>> response, int statusCode)
    {
        var result = Assert.IsAssignableFrom<ObjectResult>(response.Result);
        Assert.Equal(statusCode, result.StatusCode);
        var envelope = Assert.IsType<ApiResult<RegisterResponse>>(result.Value);
        Assert.Null(envelope.Data);
        Assert.False(string.IsNullOrWhiteSpace(envelope.Error));
        return envelope;
    }
}
