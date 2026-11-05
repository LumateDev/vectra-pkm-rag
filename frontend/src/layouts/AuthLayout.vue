<script setup lang="ts">
import ThemeToggle from '../components/ThemeToggle.vue';
</script>

<template>
  <q-layout view="hHh lpR fFf" class="auth-layout">
    <q-page-container>
      <q-page class="auth-page">
        <header class="auth-header">
          <div class="brand">
            <span class="brand-symbol" aria-hidden="true">v.</span> Vectra<span class="brand-suffix"
              >PKM</span
            >
          </div>
          <ThemeToggle />
        </header>
        <main class="auth-main"><router-view /></main>
        <footer class="auth-footer">Ваше пространство для знаний.</footer>
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<style scoped lang="scss">
@use '../css/theme/contract' as theme;

.auth-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.auth-page {
  display: flex;
  flex-direction: column;
  padding: 36px 6vw 24px;
}

.brand {
  display: flex;
  align-items: center;
  gap: 9px;
  font-size: 1.3125rem;
  font-weight: 650;
  letter-spacing: -0.04375rem;
}

.brand-symbol {
  display: grid;
  place-items: center;
  width: 2.125rem;
  height: 2.125rem;
  background: theme.token('app-primary');
  border-radius: 10px;
  color: theme.token('app-on-primary');
  font-size: 1.625rem;
  line-height: 1;
}

.brand-suffix {
  color: theme.token('app-muted');
  font-weight: 400;
  margin-left: -7px;
}

.auth-main {
  display: grid;
  place-items: center;
  flex: 1;
  padding: 48px 0;
}

.auth-footer {
  text-align: center;
  font-size: 0.75rem;
  color: theme.token('app-muted');
}
@media (max-width: 480px) {
  .auth-page {
    padding: 24px 16px 20px;
  }
  .auth-main {
    padding: 32px 0;
  }
}
</style>
