import { defineStore } from 'pinia';
import { computed, onScopeDispose, ref, shallowRef } from 'vue';
import { getAuth } from '../api/generated/endpoints/auth/auth';
import type { AuthRequest, AuthResponse, RegisterRequest } from '../api/generated/models';

// Вкладки используют одну cookie. Записывающие её ответы должны приходить по очереди.
function withSessionLock<T>(action: () => Promise<T>): Promise<T> {
  const locks = globalThis.navigator?.locks;
  return locks ? locks.request('vectra-auth-session', action) : action();
}

export const useAuthStore = defineStore('auth', () => {
  const api = getAuth();
  const session = shallowRef<AuthResponse | null>(null);
  const accessToken = computed(() => session.value?.accessToken ?? null);
  const currentUser = computed(() => session.value?.user ?? null);
  const isLoggingOut = ref(false);
  const isLoggedIn = computed(() => session.value !== null);
  let initialization: Promise<void> | undefined;
  let refreshing: Promise<boolean> | undefined;
  let refreshTimer: ReturnType<typeof setTimeout> | undefined;
  let sessionVersion = 0;

  function clearAuthState() {
    clearTimeout(refreshTimer);
    sessionVersion++;
    session.value = null;
  }

  function setSession(value: AuthResponse) {
    const remaining = Date.parse(value.expiresAt) - Date.now();
    if (!value.accessToken || !value.user || !Number.isFinite(remaining) || remaining <= 0) {
      throw new Error('Сервер вернул недействительный сеанс.');
    }

    clearTimeout(refreshTimer);
    session.value = value;
    refreshTimer = setTimeout(() => void silentRefresh(), remaining);
  }

  function initialize(): Promise<void> {
    initialization ??= isLoggedIn.value ? Promise.resolve() : silentRefresh().then(() => undefined);
    return initialization;
  }

  function silentRefresh(): Promise<boolean> {
    if (isLoggingOut.value) return Promise.resolve(false);

    refreshing ??= withSessionLock(async () => {
      if (isLoggingOut.value) return false;
      const response = await api.refresh();
      if (isLoggingOut.value) return false;
      const nextSession = response?.data;
      if (
        response?.error ||
        !nextSession ||
        (currentUser.value && currentUser.value.id !== nextSession.user?.id)
      ) {
        clearAuthState();
        return false;
      }
      setSession(nextSession);
      return true;
    })
      .catch(() => {
        clearAuthState();
        return false;
      })
      .finally(() => {
        refreshing = undefined;
      });
    return refreshing;
  }

  async function register(request: RegisterRequest, signal: AbortSignal) {
    const response = await api.register(request, { signal });
    if (response.error || !response.data) {
      throw new Error(response.error || 'Сервер не подтвердил регистрацию.');
    }
  }

  function login(request: AuthRequest, signal: AbortSignal): Promise<void> {
    return withSessionLock(async () => {
      signal.throwIfAborted();
      const response = await api.login(request, { signal });
      signal.throwIfAborted();
      if (response.error || !response.data) {
        throw new Error(response.error || 'Сервер не подтвердил вход.');
      }
      setSession(response.data);
      sessionVersion++;
    });
  }

  async function logout(): Promise<void> {
    if (isLoggingOut.value) return;

    isLoggingOut.value = true;
    clearTimeout(refreshTimer);
    try {
      await refreshing;
      const response = await withSessionLock(() => api.logout());
      if (response.error || !response.data) {
        throw new Error(response.error || 'Сервер не подтвердил выход.');
      }
    } finally {
      clearAuthState();
      isLoggingOut.value = false;
    }
  }

  onScopeDispose(() => clearTimeout(refreshTimer));

  return {
    accessToken,
    currentUser,
    isLoggedIn,
    isLoggingOut,
    getSessionVersion: () => sessionVersion,
    initialize,
    silentRefresh,
    register,
    login,
    logout,
  };
});
