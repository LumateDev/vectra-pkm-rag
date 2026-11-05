import type { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('../layouts/AuthLayout.vue'),
    children: [
      { path: '', redirect: { name: 'login' } },
      { path: 'login', name: 'login', component: () => import('../pages/AuthPage.vue') },
      { path: 'register', name: 'register', component: () => import('../pages/AuthPage.vue') },
      { path: ':catchAll(.*)*', component: () => import('../pages/ErrorNotFound.vue') },
    ],
  },
];

export default routes;
