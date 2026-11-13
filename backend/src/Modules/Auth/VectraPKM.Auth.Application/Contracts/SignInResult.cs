namespace VectraPKM.Auth.Application.Contracts;

/// <summary>Данные пользователя и пара токенов после входа или обновления сессии.</summary>
public sealed record SignInResult(AuthenticationResult User, IssuedAccessToken Token, IssuedRefreshToken RefreshToken);
