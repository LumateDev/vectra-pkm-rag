# Архитектура backend

Backend использует .NET 10 (`net10.0`). Общее решение:
`backend/VectraPKM.slnx`.

## Приложения

| Приложение | Каталог в backend               | Ответственность                            |
| ---------- | ------------------------------- | ------------------------------------------ |
| Gateway    | `src/Gateway/VectraPKM.Gateway` | YARP reverse proxy и общий Swagger UI      |
| Identity   | `src/Services/Identity`         | Каркас сервиса пользователей и авторизации |

## Общие технические библиотеки

`src/Shared/VectraPKM.Logging` содержит подключение Serilog для хостов Gateway
и Identity.Api. Библиотека не содержит бизнес-логики и не используется в Domain
или Application. Подробнее: [логирование](logging.md).

## Взаимодействие сервисов

```text
Клиент → Gateway → Identity
```

Gateway — единая точка входа для клиентов. Он направляет запросы во внутренние
сервисы по HTTP. Сервисы запускаются отдельно; Gateway не ссылается на их
проекты через ProjectReference.

Доступ клиентов к внутренним сервисам предполагается только через Gateway.
В текущем локальном запуске Identity также доступен напрямую на localhost
для разработки. Сетевая изоляция при развёртывании пока не настроена.

## Документация сервисов

- [Gateway](services/gateway.md) — маршруты, конфигурация прокси и общий Swagger UI.
- [Identity](services/identity.md) — назначение, проекты сервиса и Swagger-документ.

Общий порядок запуска описан в [development.md](development.md).
