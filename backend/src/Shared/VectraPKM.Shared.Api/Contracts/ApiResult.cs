using System.Text.Json.Serialization;

namespace VectraPKM.Shared.Api.Contracts;

/// <summary>Единый HTTP-контракт: данные при успехе либо сообщение об ошибке.</summary>
/// <typeparam name="T">Тип успешного ответа.</typeparam>
public sealed class ApiResult<T>
{
    /// <summary>Создаёт ответ с данными либо сообщением об ошибке.</summary>
    public ApiResult(T? data = default, string? error = null)
    {
        Data = data;
        Error = error;
    }

    /// <summary>Данные успешного ответа; при ошибке отсутствуют.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public T? Data { get; }

    /// <summary>Сообщение об ошибке; при успехе отсутствует.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? Error { get; }
}
