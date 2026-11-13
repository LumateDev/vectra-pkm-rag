# Архитектура backend

Backend — модульный монолит на .NET 10. Решение: `backend/VectraPKM.slnx`.
Все модули выполняются в одном процессе и развёртываются вместе.

```text
Клиент → VectraPKM.Api → Auth
                     → будущие модули
```

## Хост и модули

`src/VectraPKM.Api` — единственное запускаемое приложение и точка сборки
зависимостей. Оно подключает модули, конфигурацию, Serilog и общий Swagger UI.
Gateway и YARP удалены: для одного публичного приложения они сейчас не нужны.

`src/Modules/Auth` — модуль пользователей и аутентификации на ASP.NET Core Identity.
Подробности: [Auth](modules/auth.md).

Каждый модуль имеет собственные проекты Api, Application, Domain и
Infrastructure. Domain не зависит от внешних слоёв; Application зависит от
Domain; Infrastructure реализует порты Application. Api содержит HTTP
endpoints и подключение модуля, включая регистрацию его инфраструктуры.
Хост подключает Api и вызывает методы регистрации и маршрутизации.

Соседние модули не должны обращаться к внутренним сущностям, репозиториям и
таблицам друг друга. При появлении взаимодействий определим явные контракты
модулей. Внутри процесса используем вызовы через контракты, а не HTTP между
модулями.

`src/Shared/VectraPKM.Shared.Api` содержит общий HTTP-контракт `ApiResult<T>`.
Он используется Api-проектами модулей; Domain и Application от него не зависят.

`src/Shared/VectraPKM.Shared.Infrastructure` содержит общие механизмы EF Core.
На него ссылаются Infrastructure-проекты модулей. `ForUpdate()` помечает запрос
для блокировки строк PostgreSQL, а `ForUpdateInterceptor` добавляет SQL-клаузу
при выполнении sync/async-запроса. Интерцептор регистрируется у каждого контекста:

```csharp
options.UseNpgsql(connectionString).AddInterceptors(new ForUpdateInterceptor());
```

Использование внутри явной транзакции:

```csharp
await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
var user = await db.Users.Where(user => user.Id == userId)
    .AsNoTracking().ForUpdate().SingleOrDefaultAsync(cancellationToken);
// Чтение и изменение связанных данных под блокировкой.
await transaction.CommitAsync(cancellationToken);
```

Helper рассчитан на простые SELECT сущностей PostgreSQL. Не использовать с агрегатами,
DISTINCT, GROUP BY, объединениями или split queries. Без зарегистрированного интерцептора
TagWith остаётся только комментарием и блокировка не применяется.

## Данные и будущие процессы

В Auth описаны модели и контекст EF Core. Контекст зарегистрирован в DI
В дальнейшем модули будут владеть своими таблицами и миграциями.

AI Worker можно будет выделить в отдельный процесс для фоновых заданий.
SignalR при появлении realtime-сценариев сначала размещаем в монолите.
Оба компонента пока не реализованы.

Запуск: [development.md](development.md). Логирование: [logging.md](logging.md).
