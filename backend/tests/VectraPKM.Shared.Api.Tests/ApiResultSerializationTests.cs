using System.Text.Json;
using System.Text.Json.Serialization;
using VectraPKM.Shared.Api.Contracts;
using Xunit;

namespace VectraPKM.Shared.Api.Tests;

/// <summary>Проверки JSON-контракта ApiResult без HTTP-сервера и базы данных.</summary>
public sealed class ApiResultSerializationTests
{
    /// <summary>
    /// Успешный ответ содержит data и error=null даже при общем отключении сериализации null.
    /// </summary>
    [Fact(DisplayName = "API: успешный ответ содержит data и error=null")]
    public void Success_serializes_both_envelope_fields()
    {
        var response = new ApiResult<TestData>(new TestData("Пример"));

        using var json = Serialize(response);

        Assert.Equal("Пример", json.RootElement.GetProperty("data").GetProperty("name").GetString());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("error").ValueKind);
    }

    /// <summary>
    /// Ответ с ошибкой содержит data=null и сообщение, даже если общая настройка сериализатора пропускает null.
    /// </summary>
    [Fact(DisplayName = "API: ответ с ошибкой содержит data=null и сообщение")]
    public void Error_serializes_both_envelope_fields()
    {
        var response = new ApiResult<TestData>(error: "Не удалось выполнить операцию.");

        using var json = Serialize(response);

        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("data").ValueKind);
        Assert.Equal("Не удалось выполнить операцию.", json.RootElement.GetProperty("error").GetString());
    }

    /// <summary>Сериализует ответ с веб-именованием полей и общим пропуском null для проверки контракта.</summary>
    private static JsonDocument Serialize<T>(ApiResult<T> response)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        return JsonDocument.Parse(JsonSerializer.Serialize(response, options));
    }

    /// <summary>Произвольные данные для проверки конверта, не относящиеся к какому-либо модулю.</summary>
    private sealed record TestData(string Name);
}
