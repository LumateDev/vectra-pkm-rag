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
        meta: { requiresAuth: true, projectWorkspace: true },
        children: [
          { path: '', name: 'projects', component: () => import('../pages/ProjectsPage.vue') },
          {
            path: ':id',
            name: 'project',
            component: () => import('../pages/ProjectPage.vue'),
            props: true,
          },
          {
            path: ':id/notes/new',
            name: 'new-note',
            component: () => import('../pages/NotePage.vue'),
            props: true,
          },
          {
            path: ':id/notes/:noteId',
            name: 'note',
            component: () => import('../pages/NotePage.vue'),
            props: true,
          },
        ],
      },
      { path: ':catchAll(.*)*', component: () => import('../pages/ErrorNotFound.vue') },
    ],
  },
];

export default routes;
