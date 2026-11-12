namespace VectraPKM.Auth.Application.Contracts;

/// <summary>Данные пользователя и токен после успешного входа.</summary>
public sealed record SignInResult(AuthenticationResult User, IssuedAccessToken Token);
