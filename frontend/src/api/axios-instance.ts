import axios from 'axios';
import type {
  AxiosRequestConfig,
  AxiosRequestTransformer,
  InternalAxiosRequestConfig,
} from 'axios';
import { replaceDatesWithOffset } from './axios-time-formatter';
import type { ObjectApiResult } from './generated/models';

const defaultTransform = axios.defaults.transformRequest;
const axiosTransformers = Array.isArray(defaultTransform)
  ? defaultTransform
  : defaultTransform
    ? [defaultTransform]
    : [];
const transformDates: AxiosRequestTransformer = (data: unknown) => replaceDatesWithOffset(data);
interface AxiosAuthHandlers {
  getAccessToken: () => string | null;
  getSessionVersion: () => number;
  silentRefresh: () => Promise<boolean>;
}

type SessionRequest = InternalAxiosRequestConfig & {
  sessionVersion?: number;
  retriedAfterRefresh?: boolean;
};

let authHandlers: AxiosAuthHandlers | undefined;

export function registerAxiosAuthHandlers(handlers: AxiosAuthHandlers) {
  authHandlers = handlers;
}

export const axiosInstance = axios.create({
  baseURL: (import.meta.env.QCLI_API_BASE_URL as string | undefined)?.trim() ?? '',
  timeout: 30_000,
  withCredentials: true,
  paramsSerializer: { indexes: null },
  transformRequest: [transformDates, ...axiosTransformers],
});

axiosInstance.interceptors.request.use((config: SessionRequest) => {
  config.params = replaceDatesWithOffset(config.params);
  const path = config.url;
  if (!path?.startsWith('/') || path.startsWith('//')) return config;

  if (path.startsWith('/auth/')) {
    if (config.method === 'post') config.headers.set('X-Auth-Request', '1');
    return config;
  }

  const handlers = authHandlers;
  const token = handlers?.getAccessToken();
  if (
    config.retriedAfterRefresh &&
    (!token || config.sessionVersion !== handlers?.getSessionVersion())
  ) {
    throw new axios.CanceledError('Сеанс изменился до повторной отправки запроса.');
  }
  if (handlers && token && (!config.headers.has('Authorization') || config.retriedAfterRefresh)) {
    config.headers.set('Authorization', `Bearer ${token}`);
    config.sessionVersion = handlers.getSessionVersion();
  }
  return config;
});

axiosInstance.interceptors.response.use(undefined, async (error: unknown) => {
  const handlers = authHandlers;
  if (!axios.isAxiosError(error) || error.response?.status !== 401 || !handlers) throw error;

  const request: SessionRequest | undefined = error.config;
  if (
    request?.sessionVersion === undefined ||
    request.retriedAfterRefresh ||
    request.signal?.aborted
  )
    throw error;

  // Таймер или другой запрос мог уже обновить токен после отправки этого запроса.
  const currentToken = handlers.getAccessToken();
  if (!currentToken || request.sessionVersion !== handlers.getSessionVersion()) throw error;
  if (request.headers.get('Authorization') === `Bearer ${currentToken}`) {
    if (!(await handlers.silentRefresh())) throw error;
  }
  if (
    !handlers.getAccessToken() ||
    request.sessionVersion !== handlers.getSessionVersion() ||
    request.signal?.aborted
  )
    throw error;

  request.retriedAfterRefresh = true;
  return axiosInstance.request(request);
});

export function getApiErrorMessage(error: unknown): string {
  if (!axios.isAxiosError<ObjectApiResult>(error)) {
    return error instanceof Error ? error.message : 'Не удалось выполнить запрос.';
  }

  const message = error.response?.data?.error;
  if (typeof message === 'string' && message.trim()) return message;
  if (error.response?.status === 429) return 'Слишком много запросов. Повторите попытку позже.';
  if (error.code === 'ECONNABORTED' || error.code === 'ETIMEDOUT') {
    return 'Сервер не ответил вовремя. Попробуйте позже.';
  }
  if (!error.response) return 'Не удалось связаться с сервером.';
  return 'Не удалось выполнить запрос. Попробуйте ещё раз.';
}

export async function customAxiosInstance<T>(
  config: AxiosRequestConfig,
  options?: AxiosRequestConfig,
): Promise<T> {
  const response = await axiosInstance.request<T>({
    ...config,
    ...options,
    headers: { ...config.headers, ...options?.headers },
  });
  return response.data;
}
