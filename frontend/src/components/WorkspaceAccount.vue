<script setup lang="ts">
import { computed, ref } from 'vue';
import { getApiErrorMessage } from '../api/axios-instance';
import { useNotify } from '../composables/useNotify';
import { useAppStore } from '../stores/app';
import { useAuthStore } from '../stores/auth';

const auth = useAuthStore();
const app = useAppStore();
const { notifyError } = useNotify();
const menuOpen = ref(false);
const profileOpen = ref(false);
const settingsOpen = ref(false);
const themeModes = [
  { label: 'Как в системе', value: 'system' },
  { label: 'Светлая', value: 'light' },
  { label: 'Тёмная', value: 'dark' },
];
const themeSchemes = [
  { label: 'Индиго', value: 'neutral-indigo' },
  { label: 'Графит и мята', value: 'graphite-mint' },
  { label: 'Сталь и янтарь', value: 'steel-amber' },
  { label: 'Тихий океан', value: 'pacific-ocean' },
];
const fontSizes = [
  { label: 'Мелкий', value: 'small' },
  { label: 'Стандартный', value: 'medium' },
  { label: 'Крупный', value: 'large' },
];
const name = computed(() => auth.currentUser?.userName || 'Аккаунт');
const initials = computed(() =>
  name.value
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((part) => Array.from(part)[0] ?? '')
    .join('')
    .toLocaleUpperCase('ru'),
);

async function logout() {
  try {
    await auth.logout();
  } catch (error) {
    notifyError(getApiErrorMessage(error));
  }
}
</script>

<template>
  <div class="account">
    <q-btn
      class="account-trigger"
      flat
      no-caps
      :aria-expanded="menuOpen"
      :aria-label="`Меню аккаунта: ${name}`"
      :loading="auth.isLoggingOut"
    >
      <span class="avatar" aria-hidden="true">{{ initials }}</span>
      <span class="account-name">{{ name }}</span>
      <q-icon name="unfold_more" size="1rem" aria-hidden="true" />

      <q-menu
        v-model="menuOpen"
        class="account-menu"
        anchor="top left"
        self="bottom left"
        :offset="[0, 6]"
        fit
      >
        <q-list>
          <q-item v-close-popup clickable @click="profileOpen = true">
            <q-icon name="person_outline" size="1.125rem" aria-hidden="true" />
            <q-item-section>Профиль</q-item-section>
          </q-item>
          <q-item v-close-popup clickable @click="settingsOpen = true">
            <q-icon name="settings" size="1.125rem" aria-hidden="true" />
            <q-item-section>Настройки</q-item-section>
          </q-item>
          <q-separator class="account-divider" />
          <q-item v-close-popup clickable @click="logout">
            <q-icon name="logout" size="1.125rem" aria-hidden="true" />
            <q-item-section>Выйти</q-item-section>
          </q-item>
        </q-list>
      </q-menu>
    </q-btn>

    <q-dialog v-model="profileOpen" aria-labelledby="profile-heading">
      <q-card class="account-dialog">
        <q-card-section class="dialog-heading">
          <h2 id="profile-heading">Профиль</h2>
          <q-btn
            v-close-popup
            flat
            round
            icon="close"
            size="0.875rem"
            aria-label="Закрыть профиль"
          />
        </q-card-section>
        <q-card-section>
          <dl class="profile-fields">
            <div>
              <dt>Имя пользователя</dt>
              <dd>{{ auth.currentUser?.userName }}</dd>
            </div>
            <div>
              <dt>Email</dt>
              <dd>{{ auth.currentUser?.email }}</dd>
            </div>
          </dl>
        </q-card-section>
      </q-card>
    </q-dialog>

    <q-dialog
      v-model="settingsOpen"
      aria-labelledby="settings-heading"
      @before-show="app.loadApiVersion"
    >
      <q-card class="account-dialog">
        <q-card-section class="dialog-heading">
          <h2 id="settings-heading">Настройки</h2>
          <q-btn
            v-close-popup
            flat
            round
            icon="close"
            size="0.875rem"
            aria-label="Закрыть настройки"
          />
        </q-card-section>
        <q-card-section class="appearance-settings">
          <q-select
            :model-value="app.themeMode"
            :options="themeModes"
            label="Тема оформления"
            outlined
            emit-value
            map-options
            @update:model-value="app.setThemeMode"
          />
          <q-select
            :model-value="app.themeScheme"
            :options="themeSchemes"
            label="Палитра"
            outlined
            emit-value
            map-options
            @update:model-value="app.setThemeScheme"
          />
          <q-select
            v-model="app.appFontSize"
            :options="fontSizes"
            label="Размер текста"
            outlined
            emit-value
            map-options
          />
        </q-card-section>
        <q-card-section>
          <dl class="versions" aria-label="Версии приложения">
            <div>
              <dt>Клиент</dt>
              <dd>{{ app.clientVersion }}</dd>
            </div>
            <div>
              <dt>API</dt>
              <dd>{{ app.apiVersion ?? '—' }}</dd>
            </div>
          </dl>
        </q-card-section>
      </q-card>
    </q-dialog>
  </div>
</template>

<style scoped lang="scss">
@use '../css/theme/contract' as theme;

.account {
  padding: 0.625rem;
  border-top: 1px solid theme.token('app-border');
}

.account-trigger {
  width: 100%;
  padding: 0.375rem 0.5rem;
  border-radius: 0.45rem;
  color: theme.token('app-text');
  font-size: 0.875rem;

  :deep(.q-btn__content) {
    justify-content: flex-start;
    flex-wrap: nowrap;
    gap: 0.625rem;
    min-width: 0;
  }

  :deep(.q-icon) {
    color: theme.token('app-muted');
  }
}

.avatar {
  display: grid;
  flex: 0 0 auto;
  place-items: center;
  min-width: 1.875rem;
  min-height: 1.875rem;
  padding: 0.125rem 0.25rem;
  border-radius: 0.5rem;
  background: theme.token('app-notice');
  color: theme.token('app-link');
  font-size: 0.75rem;
  font-weight: 650;
}

.account-name {
  flex: 1;
  min-width: 0;
  overflow: hidden;
  text-align: start;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.account-menu,
.account-dialog {
  border: 1px solid theme.token('app-border');
  background: theme.token('app-surface');
  color: theme.token('app-text');
}

.account-menu {
  border-radius: 0.625rem;
  box-shadow: theme.token('app-card-shadow');

  :deep(.q-item) {
    align-items: center;
    gap: 0.75rem;
    min-height: 2.75rem;
  }

  :deep(.q-icon) {
    color: theme.token('app-muted');
  }
}

.account-divider {
  background: theme.token('app-border');
}

.account-dialog {
  width: 25rem;
  border-radius: 0.875rem;
}

.dialog-heading {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
}

.dialog-heading {
  border-bottom: 1px solid theme.token('app-border');

  h2 {
    margin: 0;
    font-size: 1.25rem;
    line-height: 1.25;
    font-weight: 650;
  }
}

.profile-fields {
  display: grid;
  gap: 1rem;
  margin: 0;

  dt {
    color: theme.token('app-muted');
  }

  dd {
    margin: 0.2rem 0 0;
    overflow-wrap: anywhere;
  }
}

.appearance-settings {
  display: grid;
  gap: 0.75rem;
}

.versions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem 1.25rem;
  margin: 0;
  color: theme.token('app-muted');
  font-size: 0.8125rem;

  div {
    display: flex;
    gap: 0.375rem;
  }

  dd {
    margin: 0;
  }
}
</style>
