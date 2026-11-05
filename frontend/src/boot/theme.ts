import { defineBoot } from '#q-app';
import { Dark } from 'quasar';

export const themeStorageKey = 'vectra.theme';

export default defineBoot(() => {
  try {
    const saved = localStorage.getItem(themeStorageKey);
    Dark.set(saved === 'dark' ? true : saved === 'light' ? false : 'auto');
  } catch {
    Dark.set('auto');
  }
});
