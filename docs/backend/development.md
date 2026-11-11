# Разработка backend

Требуется стабильный .NET 10 SDK. Из корня репозитория:

```powershell
cd backend
dotnet build VectraPKM.slnx
dotnet run --project src/VectraPKM.Api --launch-profile http
```

В Visual Studio запускай только `VectraPKM.Api`.

- HTTP: http://localhost:5000
- HTTPS в профиле https: https://localhost:7000 (HTTP 5000 тоже включён).
- Swagger UI: http://localhost:5000/swagger
- OpenAPI: http://localhost:5000/swagger/v1/swagger.json

Swagger включён только в Development. В нём доступен `POST /auth/register`.
Для HTTPS нужен доверенный сертификат: `dotnet dev-certs https --trust`.
Для регистрации требуется доступная PostgreSQL с применённой миграцией Auth
и строка подключения `ConnectionStrings:Auth`. При старте миграции не применяются.
Ключ JWT пока не требуется.

## Конфигурация

Конфигурация находится в хосте `src/VectraPKM.Api`. Все три файла хранятся в Git:

- `appsettings.json` — общие настройки логирования, AllowedHosts и ключ ConnectionStrings:Auth.
- `appsettings.Development.json` — переопределение формата логов для разработки.
- `appsettings.Production.json` — пока пустой файл для будущих production-настроек.

.NET загружает общий файл и файл текущего окружения; окруженческие значения
переопределяют общие. Production не загружает Development.
Example-файлов и User Secrets нет. Development исключён из публикации.

`Properties/launchSettings.json` задаёт локальные профили и окружение Development.
Release/Debug — конфигурация сборки, она не выбирает окружение приложения.

### Подготовка инструментов

Установить EF CLI и добавить пакет инструментов в запускаемый проект:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.13
dotnet add src/VectraPKM.Api package Microsoft.EntityFrameworkCore.Design --version 10.0.13
```

Если `dotnet-ef` уже установлен, вместо установки обнови его:

```powershell
dotnet tool update --global dotnet-ef --version 10.0.13
```

### Создание миграции

Создать начальную миграцию:

```powershell
dotnet ef migrations add InitialAuth `
  --project src/Modules/Auth/VectraPKM.Auth.Infrastructure `
  --startup-project src/VectraPKM.Api `
  --context AuthDbContext `
  --output-dir Persistence/Migrations `
  -- --environment Development
```

Команда создаёт файлы миграции и снимок модели в Infrastructure, но не изменяет БД.
Для следующих изменений используй эту же команду с новым описательным именем
вместо `InitialAuth`. Уже созданную начальную миграцию повторно создавать не нужно.

### Применение миграций

Применить все ещё не применённые миграции к базе из строки подключения:

```powershell
dotnet ef database update `
  --project src/Modules/Auth/VectraPKM.Auth.Infrastructure `
  --startup-project src/VectraPKM.Api `
  --context AuthDbContext `
  -- --environment Development
```

### Просмотр миграций

```powershell
dotnet ef migrations list `
  --project src/Modules/Auth/VectraPKM.Auth.Infrastructure `
  --startup-project src/VectraPKM.Api `
  --context AuthDbContext `
  -- --environment Development
```

Подробнее: [Auth](modules/auth.md).

## Тесты

Из каталога `backend`:

```shell
dotnet test tests/VectraPKM.Auth.Tests
```

Тесты общего HTTP-контракта, не зависящие от Auth и других модулей:

```shell
dotnet test tests/VectraPKM.Shared.Api.Tests
```

В `VectraPKM.Shared.Api.Tests` сериализация `ApiResult<T>` проверяется один раз
на простой тестовой модели. В тестах модулей проверяется их собственное поведение;
повторять проверки общего конверта для каждого модуля не нужно.


Автоматическая проверка HTTP-модели выполняется до вызова `AuthService`;
Identity повторно проверяет свои правила при создании пользователя.
Контроллер возвращает `ApiResult<T>` для успеха и ожидаемых ошибок сервиса.
Автоматические ошибки MVC пока остаются в стандартном формате ASP.NET Core:
общего преобразования ошибок нет. Неожиданные исключения также не перехватываются
контроллером. Поле `error` в `ApiResult` содержит только строку сообщения.
