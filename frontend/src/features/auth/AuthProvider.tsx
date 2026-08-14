import { useCallback, useEffect, useState, type ReactNode } from 'react';
import { AuthContext, type AuthState } from './AuthContext';
import { FullPageLoader } from '../../components/FullPageLoader';
import { tokenStore } from '../../lib/api/tokenStore';
import * as authApi from './api';
import type { LoginRequest } from './types';

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [state, setState] = useState<AuthState>({ status: 'initializing' });

  // Registers the transport-layer hook once. Kept out of React state so
  // lib/api/* never needs to import React or the router — see client.ts.
  useEffect(() => {
    tokenStore.setOnSessionExpired(() => {
      setState({ status: 'unauthenticated' });
    });
  }, []);

  // Boot-time session restore: the access token never survives a reload
  // (in-memory only), so every fresh page load re-derives it from the
  // HttpOnly refresh cookie before rendering any route.
  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        await authApi.refresh();
        const user = await authApi.me();
        if (!cancelled) {
          setState({ status: 'authenticated', user });
        }
      } catch {
        if (!cancelled) {
          setState({ status: 'unauthenticated' });
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  const login = useCallback(async (request: LoginRequest) => {
    await authApi.login(request);
    const user = await authApi.me();
    setState({ status: 'authenticated', user });
  }, []);

  const logout = useCallback(async () => {
    await authApi.logout();
    setState({ status: 'unauthenticated' });
  }, []);

  // Gate every route — public and protected alike — until the boot-time
  // restore resolves, so an authenticated user never sees a login flash.
  if (state.status === 'initializing') {
    return <FullPageLoader />;
  }

  return <AuthContext.Provider value={{ state, login, logout }}>{children}</AuthContext.Provider>;
}
