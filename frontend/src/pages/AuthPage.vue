<template>
  <section class="auth-card" aria-labelledby="auth-title">
    <div class="auth-heading">
      <div class="eyebrow">НАЧНИТЕ С ГЛАВНОГО</div>
      <h1 id="auth-title">{{ isRegister ? 'Создайте аккаунт' : 'С возвращением' }}</h1>
      <p>
        {{
          isRegister ? 'Соберите свои знания в одном месте.' : 'Войдите в своё пространство знаний.'
        }}
      </p>
    </div>
    <nav class="auth-tabs" aria-label="Доступ к аккаунту">
      <router-link :to="{ name: 'login' }" :class="{ active: !isRegister }">Вход</router-link>
      <router-link :to="{ name: 'register' }" :class="{ active: isRegister }"
        >Регистрация</router-link
      >
    </nav>
    <q-form ref="form" class="auth-form" @submit="onSubmit" @validation-error="submitted = false">
      <q-input
        v-model.trim="email"
        outlined
        label="Email"
        type="email"
        autocomplete="email"
        name="email"
        :rules="[emailRule]"
        lazy-rules
        hide-bottom-space
        maxlength="254"
        @update:model-value="submitted = false"
      />
      <q-input
        v-model="password"
        outlined
        label="Пароль"
        :type="showPassword ? 'text' : 'password'"
        :autocomplete="isRegister ? 'new-password' : 'current-password'"
        name="password"
        :rules="isRegister ? [passwordRule] : [requiredRule]"
        :hint="isRegister ? 'От 8 до 128 символов' : undefined"
        lazy-rules
        hide-bottom-space
        @update:model-value="submitted = false"
      >
        <template #append>
          <q-btn
            flat
            round
            dense
            size="0.75rem"
            :icon="showPassword ? 'visibility_off' : 'visibility'"
            :aria-label="showPassword ? 'Скрыть пароль' : 'Показать пароль'"
            :aria-pressed="showPassword"
            @click="showPassword = !showPassword"
          />
        </template>
      </q-input>
      <q-input
        v-if="isRegister"
        v-model="confirmation"
        outlined
        label="Повторите пароль"
        :type="showPassword ? 'text' : 'password'"
        autocomplete="new-password"
        name="password-confirmation"
        :rules="[requiredRule, (value: string) => value === password || 'Пароли не совпадают']"
        lazy-rules
        hide-bottom-space
        reactive-rules
        @update:model-value="submitted = false"
      />
      <q-btn
        class="auth-submit"
        color="primary"
        unelevated
        no-caps
        type="submit"
        :label="isRegister ? 'Создать аккаунт' : 'Войти'"
        icon-right="arrow_forward"
      />
      <q-banner v-if="submitted" class="auth-notice" rounded role="status">
        Данные проверены.
        {{ isRegister ? 'Регистрация станет доступна' : 'Вход станет доступен' }} после подключения
        сервера.
      </q-banner>
    </q-form>
    <p class="auth-switch">
      {{ isRegister ? 'Уже есть аккаунт?' : 'Впервые здесь?' }}
      <router-link :to="{ name: isRegister ? 'login' : 'register' }">{{
        isRegister ? 'Войти' : 'Создать аккаунт'
      }}</router-link>
    </p>
  </section>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useMeta } from 'quasar';
import type { QForm } from 'quasar';
import { useRoute } from 'vue-router';
import { emailRule, passwordRule, requiredRule } from '../utils/validation';

const route = useRoute();
const isRegister = computed(() => route.name === 'register');
const form = ref<QForm>();
const email = ref('');
const password = ref('');
const confirmation = ref('');
const showPassword = ref(false);
const submitted = ref(false);

useMeta(() => ({ title: `${isRegister.value ? 'Регистрация' : 'Вход'} · VectraPKM` }));

watch(isRegister, () => {
  password.value = '';
  confirmation.value = '';
  showPassword.value = false;
  submitted.value = false;
  form.value?.resetValidation();
});

function onSubmit() {
  submitted.value = true;
}
</script>

<style scoped lang="scss">
@use '../css/theme/contract' as theme;

.auth-card {
  width: 100%;
  max-width: 440px;
  background: theme.token('app-surface');
  border: 1px solid theme.token('app-border');
  border-radius: 20px;
  padding: 36px;
  box-shadow: theme.token('app-card-shadow');
}

.eyebrow {
  color: theme.token('app-muted');
  font-size: 0.625rem;
  font-weight: 650;
  letter-spacing: 0.1rem;
}

h1 {
  font-size: 1.75rem;
  line-height: 1.25;
  letter-spacing: -0.05625rem;
  font-weight: 600;
  margin: 13px 0 10px;
}

.auth-heading p {
  color: theme.token('app-muted');
  font-size: 0.875rem;
  margin: 0;
  line-height: 1.6;
}

.auth-tabs {
  display: flex;
  gap: 4px;
  padding: 4px;
  background: theme.token('app-surface-soft');
  border-radius: 10px;
  margin: 28px 0 24px;
}

.auth-tabs a {
  flex: 1;
  text-align: center;
  text-decoration: none;
  padding: 9px 4px;
  border-radius: 7px;
  font-size: 0.8125rem;
  font-weight: 500;
  color: theme.token('app-muted');
}

.auth-tabs a.active {
  background: theme.token('app-surface');
  color: theme.token('app-text');
  box-shadow: theme.token('app-tab-shadow');
}

.auth-form {
  display: flex;
  flex-direction: column;
  gap: 20px;

  :deep(.q-field) {
    font-size: 0.875rem;
  }

  :deep(.q-field__bottom) {
    font-size: 0.75rem;
    line-height: 1.4;
  }
}

.auth-form :deep(.q-field__control) {
  border-radius: 10px;
}

.auth-form
  :deep(.q-field--outlined:not(.q-field--error):not(.q-field--focused) .q-field__control:before) {
  border-color: theme.token('app-border');
}

.auth-form :deep(.q-field__label) {
  font-size: 0.875rem;
}

// Browser autofill backgrounds override background-color; cover them with
// an inset shadow while keeping the native autocomplete behavior.
.auth-form :deep(input:is(:autofill, :-webkit-autofill)) {
  box-shadow: 0 0 0 1000px theme.token('app-surface') inset;
  -webkit-text-fill-color: theme.token('app-text');
  caret-color: theme.token('app-text');
}

.auth-submit {
  min-height: 3rem;
  border-radius: 10px;
  font-size: 0.875rem;
  margin-top: 4px;
  color: theme.token('app-on-primary');
}

.auth-submit :deep(.q-icon) {
  font-size: 1.125rem;
}

.auth-notice {
  background: theme.token('app-notice');
  color: theme.token('app-link');
  font-size: 0.8125rem;
  line-height: 1.6;
}

.auth-switch {
  text-align: center;
  color: theme.token('app-muted');
  font-size: 0.8125rem;
  margin: 28px 0 0;
}

.auth-switch a {
  font-weight: 500;
  margin-left: 4px;
}

@media (max-width: 480px) {
  .auth-card {
    padding: 28px 22px;
    border-radius: 16px;
  }
  h1 {
    font-size: 1.625rem;
  }
}
</style>
