# Identity

Модуль пока содержит только четыре пустых проекта на .NET 10.
Каталог: `backend/src/Modules/Identity`.

| Проект | Будущее назначение | Прямые ссылки |
| --- | --- | --- |
| `VectraPKM.Identity.Api` | HTTP-контроллеры и подключение модуля | Application, Infrastructure |
| `VectraPKM.Identity.Application` | Сценарии приложения | Domain |
| `VectraPKM.Identity.Domain` | Доменные сущности и правила | Нет |
| `VectraPKM.Identity.Infrastructure` | Работа с хранилищем и внешними системами | Application, Domain |

Все проекты — библиотеки. Запускается только общий `VectraPKM.Api`.
Исходный код Identity, endpoints, миграции, настройки JWT и зависимости
ASP.NET Core Identity/EF Core/Npgsql удалены. Подключение модуля в хосте
будет добавлено при реализации первого сценария. База PostgreSQL не изменялась.
