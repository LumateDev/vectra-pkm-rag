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
Email должен быть уникальным. Имя пользователя обязательно, уникально без учёта
регистра и содержит от 3 до 32 символов: латинские буквы, цифры, точку, дефис
и подчёркивание. Пробелы, кириллица и `@` не допускаются. Исходный регистр имени
сохраняется; для поиска и уникальности Identity использует нормализованное имя.
Длину проверяет `UserNameLengthValidator`, остальные правила — стандартный
валидатор Identity и уникальный индекс нормализованного имени в БД.

Пароль — от 8 до 128 символов без обязательных
цифр, букв разного регистра и спецсимволов. Максимальную длину проверяет
`MaximumPasswordLengthValidator` в дополнение к стандартному валидатору Identity.

Настройки lockout: для новых пользователей блокировка разрешена,
порог — 5 неудачных попыток, продолжительность — 15 минут.
При входе проверяется блокировка, неверный пароль увеличивает счётчик попыток,
а успешная проверка пароля сбрасывает его через Identity.

Хеширование выполняет встроенный `PasswordHasher<ApplicationUser>`,
который регистрируется через `AddIdentityCore`. Собственного провайдера и адаптера нет.
Соль, параметры PBKDF2 и хеш хранятся в штатном формате Identity.
При успешной проверке пароля с устаревшими параметрами `UserManager`
обновляет сохранённый хеш. Парольная политика проверяется отдельными валидаторами.

Тесты в `backend/tests/VectraPKM.Auth.Tests` проверяют хеширование, неверные
пароли, случайную соль, обновление хеша и парольную политику через UserManager.
Тесты используют настоящий `AuthDbContext` и EF-хранилище Identity с провайдером
`Microsoft.EntityFrameworkCore.InMemory`; PostgreSQL не требуется. Каждый тестовый
набор сервисов получает отдельную БД с уникальным именем. Сохранённые данные
проверяются через новый контекст без отслеживания изменений. Провайдер InMemory
не проверяет ограничения PostgreSQL, SQL, миграции и конкурентные запросы.

## Регистрация

`POST /auth/register` принимает `email`, `userName` и `password`.
Успех — `201` с `ApiResult<RegisterResponse>`: данные `userId`, `email` и `userName`
находятся в `data`, поле `error` равно null. Вход, cookie и выдача токенов не выполняются.
Ошибки Identity, обработанные контроллером, возвращают `400`,
занятый email или username — `409`. В этих ответах `data` равно null,
а `error` содержит сообщение. Автоматические ошибки HTTP-модели MVC также возвращаются как ApiResult.
Уникальность проверяется Identity и индексами PostgreSQL, включая конкурентные регистрации.

Контроллер обращается к `IAuthService` / `AuthService` в Application.
Сервис использует интерфейс `IUserRegistration`, реализованный в Infrastructure
через `UserManager<ApplicationUser>`. Типы Identity и HTTP не выходят в Application.
HTTP-контракты находятся в Api; между слоями используются отдельные record-контракты.

Входные данные сервиса называются `RegistrationData`, успешный результат —
`RegistrationResult`. Ожидаемые ошибки передаются через `AuthValidationException`
и `AuthConflictException`. Контроллер явно перехватывает эти два типа
и переводит их в HTTP-статусы и `ApiResult`.
Названия Request/Response используются только для HTTP-контрактов.

Общий проект `VectraPKM.Shared.Api` содержит контракт `ApiResult<T>`.
В хосте настроены ответы валидации и общий обработчик исключений: подробности
неожиданных ошибок остаются в серверном логе, клиент получает нейтральный ответ 500.
Сервисный слой не зависит от HTTP-обёртки. В `ApiResult` есть только строка ошибки:
машинные коды и отдельные ошибки полей в него не включены.


## Авторизация

`POST /auth/login` (метод контроллера `Auth`) принимает `login` и `password`.
`login` содержит username или email. Значение с `@` ищется по email, остальные —
по username: символ `@` запрещён в username правилами регистрации.
Оба варианта ищутся без учёта регистра средствами Identity; пароль проверяется без
обрезания пробелов. Успех — `200` с `ApiResult<AuthResponse>`:

```json
{
  "data": {
    "accessToken": "<JWT>",
    "expiresAt": "2026-11-12T12:15:00Z",
    "user": {
      "id": "e3580e25-e88d-4fa2-bf79-02809aeb47d3",
      "email": "alex@example.com",
      "userName": "Alex"
    }
  },
  "error": null
}
```

`accessToken` — JWT с подписью HS256 и сроком из Jwt:AccessTokenLifetimeMinutes (по умолчанию 15 минут). Claims: `sub` (ID пользователя),
`sid` (ID сессии), `jti`, `iat`, `nbf`, `iss`, `aud`, `exp`. Email и username возвращаются только в `user`.
Каждый успешный вход получает новый `sid` и семейство refresh-токенов в существующей
таблице `RefreshTokens`. Исходный refresh передаётся только через HttpOnly-cookie,
в JSON его нет, а в БД сохраняется только SHA-256-хеш.
SPA хранит access-токен в памяти и передаёт `Authorization: Bearer <JWT>` по HTTPS.
API проверяет подпись, алгоритм HS256, issuer, audience и срок без ClockSkew.
Общая политика требует авторизованного пользователя; вход и регистрация помечены AllowAnonymous.
Неверный пароль, неизвестный логин и заблокированная учётная запись дают одинаковый
`401`: `data` равно null, `error` — «Неверный логин или пароль.».
Ошибки HTTP-модели возвращают `400`. Ответы контроллера имеют `Cache-Control: no-store`.

Цепочка вызовов: `AuthController.Auth` → `AuthService.AuthAsync` →
`IUserAuthentication`, реализованный `IdentityUserAuthentication` через `UserManager`.
После проверки пароля `AuthService` выпускает токен через `IAccessTokenIssuer`;
реализация `JwtAccessTokenIssuer` находится в Infrastructure.
Прикладные контракты — `AuthenticationData`, `AuthenticationResult` (проверенный пользователь),
`SignInResult` (пользователь и пара токенов), `IssuedAccessToken` и `IssuedRefreshToken` (значение и срок),
ожидаемый отказ — `AuthUnauthorizedException`.

Тесты `AuthenticationTests` проверяют успешный вход по username и email, JWT и его claims,
отказ при неправильной подписи, алгоритме, issuer, audience и истёкшем сроке,
неверные учётные данные, сохранение пробелов пароля, блокировку после пяти ошибок,
вход после окончания блокировки, сброс счётчика и отмену запроса.
Контроллер вызывается напрямую, без HTTP-хоста и PostgreSQL.

JWT challenge и запрет доступа возвращают ApiResult с нейтральными сообщениями 401/403.

## Refresh и logout

- `POST /auth/login` сохраняет refresh и устанавливает cookie `refreshToken`.
- `POST /auth/refresh` читает cookie, помечает её токен использованным и выдаёт новую пару.
  Ответ — тот же `ApiResult<AuthResponse>`, что при входе. `sid` и абсолютный срок
  refresh-сессии сохраняются. Неизвестный, истёкший или отозванный токен даёт 401
  с очисткой cookie. Повтор использованного токена отзывает всё семейство до возврата 401.
- `POST /auth/logout` доступен без JWT, отзывает семейство по cookie и удаляет её.
  Возвращает `200` с `ApiResult<bool>(true)` даже при отсутствующей или неизвестной cookie.
  Можно передать ранее ротированный токен: будет отозвано всё его семейство.

Cookie: `HttpOnly`, `Secure`, `SameSite=Strict`, `Path=/auth`, без Domain.
Срок задаётся `Jwt:RefreshTokenLifetimeDays` с момента входа; ротация его не продлевает.
Cookie устанавливается и удаляется с одинаковыми атрибутами. Локальная проверка — по HTTPS.

Обновление и отзыв выполняются транзакционно. `SELECT FOR UPDATE` блокирует строку
пользователя в `AspNetUsers`; после блокировки refresh перечитывается. Это согласует
операции всех сессий пользователя между процессами без новой таблицы сессий.
Блокировка задаётся через общий `ForUpdate()` и зарегистрированный интерцептор.
Первый запрос refresh выбирает только Id/UserId/SessionId. После блокировки второй
запрос читает актуальную сущность через AsNoTracking без дополнительного ReloadAsync.
Отметка использования сохраняется ExecuteUpdateAsync, новый refresh — SaveChangesAsync;
обе операции входят в одну транзакцию. Результаты bulk update не синхронизируют Change Tracker.
Использованные записи сохраняются для reuse detection. Фоновая очистка пока не подключена.
Выданный JWT после logout/reuse действует до exp: сервер не проверяет отзыв сессии на каждом запросе.

