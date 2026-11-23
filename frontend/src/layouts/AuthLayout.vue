<script setup lang="ts">
import AppBrand from '../components/AppBrand.vue';
import ThemeToggle from '../components/ThemeToggle.vue';
</script>

<template>
  <q-layout view="hHh lpR fFf">
    <q-page-container>
      <q-page class="auth-page">
        <header class="auth-header">
          <AppBrand />
          <ThemeToggle />
        </header>
        <div class="auth-main"><router-view /></div>
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
  font-size: 1.3125rem;
}

.auth-page {
  display: flex;
  flex-direction: column;
  padding: 36px 6vw 24px;
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
