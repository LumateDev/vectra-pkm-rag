import { defineBoot } from '#q-app';
import { Dark } from 'quasar';
import { watch } from 'vue';
import { useAppStore } from '../stores/app';

export default defineBoot(({ store }) => {
  const app = useAppStore(store);

  watch(
    () => app.isDark,
    (isDark) => Dark.set(isDark),
    { immediate: true },
  );
  watch(
    () => [app.themeScheme, app.appFontSize] as const,
    ([scheme, size]) => {
      document.documentElement.dataset.themeScheme = scheme;
      document.documentElement.dataset.appFontSize = size;
    },
    { immediate: true },
  );
});
