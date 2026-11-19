import { defineRouter } from '#q-app';
import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import routes from './routes';

export default defineRouter(({ store }) => {
  const authStore = useAuthStore(store);
  const router = createRouter({
    history: createWebHistory(import.meta.env.QUASAR_VUE_ROUTER_BASE),
    routes,
    scrollBehavior: () => ({ left: 0, top: 0 }),
  });

  router.beforeEach(async (to) => {
    await authStore.initialize();

    // Закрытые страницы требуют активного сеанса.
    if (to.meta.requiresAuth && !authStore.isLoggedIn) {
      return { name: 'login' };
    }

    // После входа возвращаем пользователя из гостевых страниц к проектам.
    if (to.meta.guestOnly && authStore.isLoggedIn) {
      return { name: 'projects' };
    }
  });

  return router;
});
