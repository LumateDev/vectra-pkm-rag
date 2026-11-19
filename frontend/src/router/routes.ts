import type { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('../layouts/AuthLayout.vue'),
    children: [
      { path: '', redirect: { name: 'login' } },
      {
        path: 'login',
        name: 'login',
        component: () => import('../pages/AuthPage.vue'),
        meta: { guestOnly: true },
      },
      {
        path: 'register',
        name: 'register',
        component: () => import('../pages/AuthPage.vue'),
        meta: { guestOnly: true },
      },
      {
        path: 'projects',
        name: 'projects',
        component: () => import('../pages/ProjectsPage.vue'),
        meta: { requiresAuth: true },
      },
      { path: ':catchAll(.*)*', component: () => import('../pages/ErrorNotFound.vue') },
    ],
  },
];

export default routes;
