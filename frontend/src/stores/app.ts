import { usePreferredDark, useStorage } from '@vueuse/core';
import { defineStore } from 'pinia';
import { computed, ref } from 'vue';
import { version as clientVersion } from '../../package.json';
import { getSystem } from '../api/generated/endpoints/system/system';

const themeModes = ['system', 'light', 'dark'] as const;
const themeSchemes = [
  'neutral-indigo',
  'graphite-mint',
  'steel-amber',
  'pacific-ocean',
  'amnezia-anarchy',
] as const;
const fontSizes = ['small', 'medium', 'large'] as const;

export type ThemeMode = (typeof themeModes)[number];
export type ThemeScheme = (typeof themeSchemes)[number];
export type AppFontSize = (typeof fontSizes)[number];

function usePreference<T extends string>(key: string, fallback: T, choices: readonly T[]) {
  return useStorage<T>(key, fallback, undefined, {
    serializer: {
      read: (stored) => choices.find((choice) => choice === stored) ?? fallback,
      write: (value) => value,
    },
    onError: () => {
      // Настройки продолжают работать в памяти, если хранилище недоступно.
    },
  });
}

export const useAppStore = defineStore('app', () => {
  const system = getSystem();
  const themeMode = usePreference('vectra.theme', 'system', themeModes);
  const themeScheme = usePreference('vectra.theme-scheme', 'neutral-indigo', themeSchemes);
  const appFontSize = usePreference('vectra.font-size', 'medium', fontSizes);
  const prefersDark = usePreferredDark();
  const isDark = computed(() =>
    themeMode.value === 'system' ? prefersDark.value : themeMode.value === 'dark',
  );
  const apiVersion = ref<string | null>(null);

  function setThemeMode(mode: ThemeMode) {
    themeMode.value = mode;
  }

  function toggleTheme() {
    setThemeMode(isDark.value ? 'light' : 'dark');
  }

  function setThemeScheme(scheme: ThemeScheme) {
    themeScheme.value = scheme;
  }

  async function loadApiVersion() {
    try {
      const response = await system.getVersion();
      apiVersion.value = response.data ?? 'Недоступна';
    } catch {
      apiVersion.value = 'Недоступна';
    }
  }

  return {
    themeMode,
    themeScheme,
    appFontSize,
    isDark,
    clientVersion,
    apiVersion,
    setThemeMode,
    toggleTheme,
    setThemeScheme,
    loadApiVersion,
  };
});
