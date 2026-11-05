<script setup lang="ts">
import { useQuasar } from 'quasar';
import { computed } from 'vue';
import { themeStorageKey } from '../boot/theme';

const $q = useQuasar();
const label = computed(() => ($q.dark.isActive ? 'Включить светлую тему' : 'Включить тёмную тему'));

function toggleTheme() {
  $q.dark.toggle();
  try {
    localStorage.setItem(themeStorageKey, $q.dark.isActive ? 'dark' : 'light');
  } catch {
    // The switch still works when browser storage is unavailable.
  }
}
</script>

<template>
  <q-btn
    class="theme-toggle"
    flat
    round
    size="0.875rem"
    :icon="$q.dark.isActive ? 'light_mode' : 'dark_mode'"
    :aria-label="label"
    :title="label"
    @click="toggleTheme"
  />
</template>

<style scoped lang="scss">
@use '../css/theme/contract' as theme;

.theme-toggle {
  color: theme.token('app-text');
}
</style>
