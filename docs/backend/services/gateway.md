# Gateway

Gateway — отдельное ASP.NET Core приложение на YARP, единая точка входа
в backend. Проект: `backend/src/Gateway/VectraPKM.Gateway`.

## Маршрутизация

Маршруты и адреса назначения находятся в секции `ReverseProxy` файла
`appsettings.json` Gateway.

| Входящий путь | Сервис назначения | Преобразование пути |
| --- | --- | --- |
| `/api/identity/{**catch-all}` | Identity, `http://localhost:5001/` | Удаление `/api/identity` |

Gateway устанавливает `X-Forwarded-Prefix: /api/identity`. Identity использует
его для указания внешнего базового пути в Swagger-документе. Автоматическая
генерация этого заголовка YARP отключена через `Prefix: Off`, чтобы сохранить
явно заданное значение.

## Swagger UI

В Development Gateway показывает Swagger UI от Swashbuckle по адресу
http://localhost:5000/swagger.

UI загружает документ Identity через прокси:
`/api/identity/swagger/v1/swagger.json`. Gateway сам не генерирует документ
Identity и не объединяет документы в один JSON.

При добавлении сервиса необходимо настроить маршрут и кластер YARP,
а затем подключить его документ через `SwaggerEndpoint` в `Program.cs` Gateway.

Порты и общий порядок запуска: [разработка backend](../development.md).
