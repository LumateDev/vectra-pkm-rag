# Auth

Модуль учётных записей и аутентификации на .NET 10 с ASP.NET Core Identity.
Каталог: `backend/src/Modules/Auth`.

| Проект | Будущее назначение | Прямые ссылки |
| --- | --- | --- |
| `VectraPKM.Auth.Api` | HTTP-контроллеры и подключение модуля | Application, Infrastructure |
| `VectraPKM.Auth.Application` | Сценарии приложения | Domain |
| `VectraPKM.Auth.Domain` | Доменные сущности и правила | Нет |
| `VectraPKM.Auth.Infrastructure` | Работа с хранилищем и внешними системами | Application, Domain |

Все проекты — библиотеки. Запускается только общий `VectraPKM.Api`.
В Domain находится `RefreshToken`, в Infrastructure — `ApplicationUser`,
наследующий `IdentityUser<Guid>`, и `AuthDbContext` с конфигурациями EF Core.
Каталог `Infrastructure/Identity` содержит интеграцию с фреймворком Microsoft,
поэтому сохраняет название Identity.

Хост подключает модуль через `AddAuthModule`. Метод вызывает
`AddAuthInfrastructure`, который регистрирует `AuthDbContext` с провайдером
PostgreSQL и строкой подключения `ConnectionStrings:Auth`.

