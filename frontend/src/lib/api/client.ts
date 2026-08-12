import { tokenStore } from './tokenStore';
import { ApiError, toApiError, toNetworkError } from './errors';

const API_BASE = '/api';

interface ApiFetchOptions {
  /** Skip Authorization header injection and the 401-triggered refresh retry. */
  skipAuth?: boolean;
}

async function rawFetch(path: string, init: RequestInit): Promise<Response> {
  try {
    return await fetch(`${API_BASE}${path}`, {
      ...init,
      credentials: 'same-origin',
    });
  } catch {
    throw toNetworkError();
  }
}

async function doRefresh(): Promise<string> {
  const response = await rawFetch('/auth/refresh', { method: 'POST' });
  if (!response.ok) {
    throw await toApiError(response);
  }
  const data = (await response.json()) as { accessToken: string; expiresAtUtc: string };
  tokenStore.set(data.accessToken);
  return data.accessToken;
}

let inFlightRefresh: Promise<string> | null = null;

/**
 * Single-flight refresh: concurrent 401s all await the same in-flight promise
 * instead of each calling POST /api/auth/refresh, which would trip the
 * backend's refresh-token-rotation race (see Architecture.md, Known limitations).
 */
export function refreshAccessToken(): Promise<string> {
  inFlightRefresh ??= doRefresh().finally(() => {
    inFlightRefresh = null;
  });
  return inFlightRefresh;
}

export async function apiFetch(
  path: string,
  init: RequestInit = {},
  options: ApiFetchOptions = {},
  retried = false,
): Promise<Response> {
  const { skipAuth = false } = options;
  const headers = new Headers(init.headers);

  if (!skipAuth) {
    const token = tokenStore.get();
    if (token) {
      headers.set('Authorization', `Bearer ${token}`);
    }
  }

  const response = await rawFetch(path, { ...init, headers });

  if (response.status === 401 && !skipAuth && !retried) {
    try {
      await refreshAccessToken();
    } catch (error) {
      tokenStore.clear();
      tokenStore.notifySessionExpired();
      throw error instanceof ApiError ? error : toNetworkError();
    }
    return apiFetch(path, init, options, true);
  }

  if (!response.ok) {
    throw await toApiError(response);
  }

  return response;
}
