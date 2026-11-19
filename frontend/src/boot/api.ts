import { defineBoot } from '#q-app';
import { watch } from 'vue';
import { registerAxiosAuthHandlers } from '../api/axios-instance';
import { useAuthStore } from '../stores/auth';

export default defineBoot(({ store, router }) => {
  const authStore = useAuthStore(store);
  registerAxiosAuthHandlers({
    getAccessToken: () => (authStore.isLoggingOut ? null : authStore.accessToken),
    getSessionVersion: authStore.getSessionVersion,
    silentRefresh: authStore.silentRefresh,
  });

  watch(
    () => authStore.isLoggedIn,
    (isLoggedIn) => {
      if (!isLoggedIn && router.currentRoute.value.meta.requiresAuth) {
        void router.replace({ name: 'login' });
      }
    },
  );
});
