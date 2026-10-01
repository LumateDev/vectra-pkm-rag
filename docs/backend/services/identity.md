# Identity

Identity — сервис пользователей и авторизации. Сейчас создан пустой каркас:
регистрация, вход, выпуск токенов и подключение БД пока не реализованы.
Каталог: `backend/src/Services/Identity`.

## Проекты

| Проект                              | Назначение                            | Прямые ссылки на проекты    |
| ----------------------------------- | ------------------------------------- | --------------------------- |
| `VectraPKM.Identity.Api`            | HTTP endpoints, запуск и конфигурация | Application, Infrastructure |
| `VectraPKM.Identity.Application`    | Сценарии приложения                   | Domain                      |
| `VectraPKM.Identity.Domain`         | Доменные сущности и правила           | Нет                         |
| `VectraPKM.Identity.Infrastructure` | Работа с БД и внешними системами      | Application, Domain         |

## Swagger-документ

В Development Api генерирует документ через Swashbuckle:
`/swagger/v1/swagger.json`. Его отображает общий Swagger UI в Gateway.

При запросе через Gateway базовый путь в `servers` — `/api/identity`,
полученный из заголовка `X-Forwarded-Prefix`. При прямом запросе без этого
заголовка базовый путь — `/`. Поэтому запросы из общего Swagger UI направляются
через Gateway.

Порты и общий порядок запуска: [разработка backend](../development.md).
