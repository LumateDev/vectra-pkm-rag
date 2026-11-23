# Разработка backend

Требуется стабильный .NET 10 SDK. Из корня репозитория:

```powershell
cd backend
dotnet build VectraPKM.slnx
dotnet run --project src/VectraPKM.Api --launch-profile Kestrel -- --urls https://127.0.0.1:7000
```

В Visual Studio запускай `VectraPKM.Api` с профилем `Kestrel` (прямой запуск, без IIS Express).

- HTTPS: https://127.0.0.1:7000
- Swagger UI: https://127.0.0.1:7000/swagger
- OpenAPI: https://127.0.0.1:7000/swagger/v1/swagger.json

Swagger включён только в Development. В нём доступен `POST /api/auth/register`.
Для HTTPS нужен доверенный сертификат: `dotnet dev-certs https --trust`.
Для работы API нужна доступная PostgreSQL. Укажи строку подключения
`ConnectionStrings:Auth` в локальном `src/VectraPKM.Api/appsettings.Development.json`.
Перед первым запуском примени миграции по инструкции ниже.
Настройки JWT и тестовый ключ уже заданы в appsettings.json (см. ниже).

## Конфигурация

Конфигурация находится в хосте `src/VectraPKM.Api`:

- `appsettings.json` — общие настройки логирования, AllowedHosts и ключ ConnectionStrings:Auth.
- `appsettings.Development.json` — локальная копия общего файла с переопределениями для разработки:
  читаемая текстовая консоль Serilog и Kestrel на `https://127.0.0.1:7000`.
- `appsettings.Production.json` — при необходимости локальный конфиг production.


.NET загружает общий файл и файл текущего окружения; окруженческие значения
переопределяют общие. Production не загружает Development.
Example-файлов и User Secrets нет. Development исключён из публикации.

`Properties/launchSettings.json` задаёт профиль Kestrel и окружение Development.
Release/Debug — конфигурация сборки, она не выбирает окружение приложения.

### Настройки JWT

Все параметры находятся в секции `Jwt` файла `src/VectraPKM.Api/appsettings.json`:

- `Key` — общий ключ подписи HS256, не менее 32 байт UTF-8. В файле задан тестовый ключ для разработки.
- `AccessTokenLifetimeMinutes` — срок access-токена, по умолчанию 15 минут.
- `RefreshTokenLifetimeDays` — абсолютный срок refresh-сессии, по умолчанию 30 дней; ротация его не продлевает.
- `TokenCleanupIntervalHours` — интервал очистки, по умолчанию 1 час; пока не используется.
- `Issuer` — издатель, `Audience` — получатель токена.

Настройки проверяет `JwtOptions.Validate()` при регистрации сервисов.
Перед production заменить тестовый ключ своим случайным ключом.
После изменения ключа ранее выданные JWT перестают проходить проверку подписи.

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

Из каталога `backend` примени миграции Auth, затем Projects. Оба контекста
используют базу из `ConnectionStrings:Auth`. Таблица проектов ссылается на
`AspNetUsers`, таблица заметок — на проекты.

```bash
dotnet ef database update --project src/Modules/Auth/VectraPKM.Auth.Infrastructure --startup-project src/VectraPKM.Api --context AuthDbContext --configuration Release -- --environment Development
dotnet ef database update --project src/Modules/Projects/VectraPKM.Projects.Infrastructure --startup-project src/VectraPKM.Api --context ProjectsDbContext --configuration Release -- --environment Development
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
Автоматические ошибки MVC возвращаются в ApiResult. Общий обработчик исключений
возвращает нейтральный ответ 500 без стека и внутренних сообщений.
Поле `error` в `ApiResult` содержит только строку сообщения.
