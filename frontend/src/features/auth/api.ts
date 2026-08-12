import { apiFetch, refreshAccessToken } from '../../lib/api/client';
import { tokenStore } from '../../lib/api/tokenStore';
import type { LoginRequest, LoginResponse, MeResponse, RegisterRequest } from './types';

export async function register(request: RegisterRequest): Promise<void> {
  await apiFetch(
    '/auth/register',
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
    },
    { skipAuth: true },
  );
}

export async function login(request: LoginRequest): Promise<LoginResponse> {
  const response = await apiFetch(
    '/auth/login',
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
    },
    { skipAuth: true },
  );
  const data = (await response.json()) as LoginResponse;
  tokenStore.set(data.accessToken);
  return data;
}

export async function refresh(): Promise<string> {
  return refreshAccessToken();
}

export async function logout(): Promise<void> {
  try {
    await apiFetch('/auth/logout', { method: 'POST' }, { skipAuth: true });
  } finally {
    tokenStore.clear();
  }
}

export async function me(): Promise<MeResponse> {
  const response = await apiFetch('/auth/me', { method: 'GET' });
  return (await response.json()) as MeResponse;
}
