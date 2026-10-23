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

В DI также зарегистрированы сервисы Identity без ролей, включая
`UserManager<ApplicationUser>` и EF-хранилище пользователей на `AuthDbContext`.
Email должен быть уникальным. Пароль — от 8 до 128 символов без обязательных
цифр, букв разного регистра и спецсимволов. Максимальную длину проверяет
`MaximumPasswordLengthValidator` в дополнение к стандартному валидатору Identity.

Настройки lockout: для новых пользователей блокировка разрешена,
порог — 5 неудачных попыток, продолжительность — 15 минут.
Сценарий входа ещё не реализован: проверку блокировки и учёт неудачных попыток
через Identity подключим при его реализации.

