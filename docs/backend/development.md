# Разработка backend

## Сборка и запуск

Из корня VectraPKM перейди в backend:

```powershell
cd backend
dotnet build VectraPKM.slnx
```

В двух терминалах из `backend`:

```powershell
dotnet run --project src/Services/Identity/VectraPKM.Identity.Api --launch-profile http
```

```powershell
dotnet run --project src/Gateway/VectraPKM.Gateway --launch-profile http
```

В Visual Studio можно выбрать профиль одновременного запуска `Run all`, если
он настроен. Файл `VectraPKM.slnLaunch.user` содержит локальную настройку IDE.

| Приложение | HTTP                    | HTTPS в профиле https    |
| ---------- | ----------------------- | ------------------------ |
| Gateway    | `http://localhost:5000` | `https://localhost:7000` |
| Identity   | `http://localhost:5001` | `https://localhost:7001` |

Профиль `https` также включает HTTP на указанном в таблице порту.
HTTP и HTTPS должны слушать разные порты. Для HTTPS нужен доверенный сертификат
разработки (`dotnet dev-certs https --trust`).

- Swagger UI: http://localhost:5000/swagger
- Документ через Gateway: http://localhost:5000/api/identity/swagger/v1/swagger.json
- Документ напрямую: http://localhost:5001/swagger/v1/swagger.json

## Конфигурация

- `Properties/launchSettings.json` каждого приложения задаёт локальные профили,
  порты и окружение. При запуске опубликованного DLL он не применяется.
- `appsettings.json` содержит настройки приложения.
- `appsettings.Development.json` содержит переопределения для разработки.
