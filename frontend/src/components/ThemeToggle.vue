<script setup lang="ts">
import { computed } from 'vue';
import { useAppStore } from '../stores/app';

const app = useAppStore();
const label = computed(() => (app.isDark ? 'Включить светлую тему' : 'Включить тёмную тему'));
</script>

<template>
  <q-btn
    class="theme-toggle"
    flat
    round
    size="0.875rem"
    :icon="app.isDark ? 'light_mode' : 'dark_mode'"
    :aria-label="label"
    :title="label"
    @click="app.toggleTheme"
  />
</template>

<style scoped lang="scss">
@use '../css/theme/contract' as theme;

.theme-toggle {
  color: theme.token('app-text');
}
</style>
