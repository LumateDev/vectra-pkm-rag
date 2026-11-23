<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';
import ThemeToggle from '../components/ThemeToggle.vue';

const route = useRoute();
const isProjectWorkspace = computed(() => route.meta.projectWorkspace === true);
const fitWorkspace = () => ({ height: '100%', minHeight: '0' });
</script>

<template>
  <q-layout
    view="hHh lpR fFf"
    :class="{ 'workspace-layout': isProjectWorkspace }"
    :style="isProjectWorkspace ? { minHeight: '0' } : undefined"
  >
    <q-page-container :class="{ 'workspace-container': isProjectWorkspace }">
      <q-page
        class="auth-page"
        :class="{ 'workspace-page': isProjectWorkspace }"
        :style-fn="isProjectWorkspace ? fitWorkspace : undefined"
      >
        <header class="auth-header">
          <div class="brand">
            <span class="brand-symbol" aria-hidden="true">v.</span> Vectra<span class="brand-suffix"
              >PKM</span
            >
          </div>
          <ThemeToggle />
        </header>
        <div class="auth-main"><router-view /></div>
        <footer v-if="!isProjectWorkspace" class="auth-footer">
          Ваше пространство для знаний.
        </footer>
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

.workspace-layout {
  height: 100dvh;
  overflow: clip;
}

.workspace-container {
  height: 100%;
}

.workspace-page {
  gap: clamp(0.75rem, 3dvh, 2.25rem);
  padding-block: clamp(0.75rem, 2dvh, 1.5rem);

  .auth-main {
    min-height: 0;
    grid-template-rows: minmax(0, 1fr);
    place-items: stretch center;
    padding: 0;
  }
}
</style>
