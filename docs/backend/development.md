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

Swagger включён только в Development. Контроллеров пока нет: список операций
пустой. Для HTTPS нужен доверенный сертификат: `dotnet dev-certs https --trust`.
База данных и ключ JWT для запуска сейчас не требуются.

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

## Тесты паролей

Из каталога `backend`:

```shell
dotnet test tests/VectraPKM.Auth.Tests
```
