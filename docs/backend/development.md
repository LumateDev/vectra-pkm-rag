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

- `appsettings.json` — общие настройки логирования и AllowedHosts.
- `appsettings.Development.json` — переопределение формата логов для разработки.
- `appsettings.Production.json` — пока пустой файл для будущих production-настроек.

.NET загружает общий файл и файл текущего окружения; окруженческие значения
переопределяют общие. Production не загружает Development.
Example-файлов и User Secrets нет. Development исключён из публикации.

`Properties/launchSettings.json` задаёт локальные профили и окружение Development.
Release/Debug — конфигурация сборки, она не выбирает окружение приложения.

Identity очищен до пустых проектов. Авторизации, работы с БД, миграций
и проверок регистрации/входа сейчас нет. Подробнее: [Identity](modules/identity.md).
