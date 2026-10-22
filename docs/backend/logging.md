# Логирование backend

Serilog настраивается один раз в хосте `backend/src/VectraPKM.Api`.
Бывшая shared-библиотека удалена; её настройка перенесена в
`Logging/LoggingExtensions.cs` внутри хоста.

Ранний логгер пишет ошибки запуска до готовности конфигурации и DI.
Затем `AddVectraLogging` подключает окончательный логгер из секции Serilog.
`UseSerilogRequestLogging` записывает завершённые HTTP-запросы.
При завершении процесса `CloseAndFlushAsync` сбрасывает буфер логов.

События содержат ServiceName (`VectraPKM.Api`) и Environment.
Общий appsettings.json задаёт JSON-формат консоли через Serilog.Formatting.Compact.
appsettings.Development.json переопределяет его на читаемый текст.
formatter: null в Development сбрасывает formatter из общего файла.

Модули не настраивают Serilog повторно. В Application и Infrastructure при
необходимости используется `Microsoft.Extensions.Logging.ILogger<T>`
(через пакет `Microsoft.Extensions.Logging.Abstractions`). Domain не зависит
от логирования. Дополнительная обёртка над ILogger не нужна.

Используй структурированные параметры:

```csharp
logger.LogInformation("User {UserId} registered", userId);
logger.LogError(exception, "Failed to process operation {OperationId}", operationId);
```
