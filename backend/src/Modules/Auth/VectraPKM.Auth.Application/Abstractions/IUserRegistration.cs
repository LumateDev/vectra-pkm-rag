using VectraPKM.Auth.Application.Contracts;

namespace VectraPKM.Auth.Application.Abstractions;

/// <summary>
/// Граница между сценарием регистрации и инфраструктурой управления пользователями.
/// </summary>
public interface IUserRegistration
{
    /// <summary>
    /// Проверяет данные и создаёт пользователя средствами хранилища учётных записей.
    /// </summary>
    /// <param name="data">Данные новой учётной записи.</param>
    /// <param name="cancellationToken">Токен отмены до начала операции.</param>
    Task<RegistrationResult> RegisterAsync(RegistrationData data, CancellationToken cancellationToken = default);
}
