export {};

declare module 'vue-router' {
  interface RouteMeta {
    requiresAuth?: boolean;
    guestOnly?: boolean;
    projectWorkspace?: boolean;
  }
}
