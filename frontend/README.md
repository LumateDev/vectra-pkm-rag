# Frontend VectraPKM

Обязательные правила для разработки и агентов: [AGENTS.md](AGENTS.md).

Требуется Node.js 22.19+ или 24 LTS.

Запустите API с профилем Kestrel по [инструкции backend](../docs/backend/development.md).

```powershell
cd frontend
npm ci
Copy-Item .env.example .env
npm run api:generate
npm run dev
```

SPA доступна на `https://127.0.0.1:9000`. Повторяйте `npm run api:generate` после изменений OpenAPI.

```powershell
npm run typecheck
npm run lint:check
npm run build
```

`npm run format` форматирует исходники. Сборка создаётся в `dist/spa/`.
При размещении на сервере настройте fallback на `index.html` для маршрутов Vue Router (history mode).

## Стили и темы

Для каждой палитры есть отдельный Sass-файл в `src/css/theme/` с вариантами
`light` и `dark`. `_neutral-indigo.scss` задаёт полный набор токенов,
остальные палитры переопределяют нужные значения для каждого режима.
Контракт `src/css/theme/_contract.scss` перечисляет допустимые токены и описывает их назначение.
Сборка проверяет итоговую карту каждой палитры.

Режим темы, палитра и размер текста хранятся в `useAppStore` и сохраняются в браузере.
Размер текста задаётся относительным значением `--app-font-size` на `:root`.

| Токен              | Назначение                      |
| ------------------ | ------------------------------- |
| `app-background`   | Фон приложения                  |
| `app-surface`      | Карточки и активные вкладки     |
| `app-surface-soft` | Второстепенные подложки         |
| `app-text`         | Основной текст                  |
| `app-muted`        | Второстепенный текст            |
| `app-border`       | Границы                         |
| `app-link`         | Ссылки и фокус                  |
| `app-primary`      | Основное действие и знак бренда |
| `app-on-primary`   | Текст на основном действии      |
| `app-negative`     | Ошибки                          |
| `app-notice`       | Фон сообщения                   |
| `app-card-shadow`  | Тень карточки                   |
| `app-tab-shadow`   | Тень активной вкладки           |

Например, фон тёмной палитры «Индиго» задаётся токеном `app-background` в варианте `dark` файла `_neutral-indigo.scss`.

Стили компонента пишем в его `<style scoped lang="scss">`:

```scss
@use '../css/theme/contract' as theme;

.card {
  background: theme.token('app-surface');
  color: theme.token('app-text');
}
```

`theme.token()` проверяет имя и возвращает CSS-переменную, например `var(--app-surface)`.
Значения меняются при переключении темы без пересоздания компонентов.
Для нового токена сначала добавьте описание в контракт и значения в обе палитры.
Цветовые литералы храним в палитрах; локальные стили используют семантические токены.

`src/css/theme/_index.scss` выводит CSS-переменные и связывает их с `--q-primary`, `--q-negative`, `--q-dark` и `--q-dark-page` Quasar.
`app.scss` содержит только подключение темы, базовую типографику и поведение ссылок.
Отступы, размеры и адаптивные правила принадлежат компонентам; `:deep()` применяем только для внутренних элементов Quasar.
