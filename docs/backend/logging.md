# Логирование backend

Сервисы используют Serilog через общую библиотеку
`backend/src/Shared/VectraPKM.Logging`. Только хосты ссылаются на неё.
Domain не зависит от логирования. В Application и Infrastructure при необходимости
используется `Microsoft.Extensions.Logging.ILogger<T>`; собственная обёртка не нужна.
Для библиотек классов абстракции доступны через пакет
`Microsoft.Extensions.Logging.Abstractions`.

## Подключение логирования для нового сервиса

1. Добавить ProjectReference на `VectraPKM.Logging` в проект хоста.
2. До `WebApplication.CreateBuilder` создать ранний логгер:
   `Log.Logger = LoggingExtensions.CreateBootstrapLogger("ServiceName")`.
3. Вызвать `builder.AddVectraLogging("ServiceName")`.
4. После `builder.Build()` вызвать `app.UseSerilogRequestLogging()` перед
   Swagger и обработчиками запросов.
5. Запускать приложение внутри `try/catch/finally`: записать ошибку запуска
   через `Log.Fatal`, установить ненулевой `Environment.ExitCode`, а в `finally`
   вызвать `await Log.CloseAndFlushAsync()`. Пример — Program.cs существующих хостов.
6. Перенести секцию `Serilog` из конфигурации существующего хоста.

Ранний логгер пишет в консоль до доступности конфигурации и DI. После подключения
хоста его заменяет окончательный логгер, читающий секцию `Serilog` и DI.
Окончательные события содержат `ServiceName` и `Environment`.

## Конфигурация

`appsettings.json` задаёт уровень Information, подавляет шум ASP.NET Core до
Warning и выводит структурированный JSON в stdout через Console sink.
`appsettings.Development.json` переключает консоль на читаемый текст.
`formatter: null` в Development сбрасывает JSON formatter базовой конфигурации.

## Правила записи

Структурированные параметры вместо интерполяции строк:

```csharp
logger.LogInformation("User {UserId} registered", userId);
logger.LogError(exception, "Failed to process operation {OperationId}", operationId);
```
