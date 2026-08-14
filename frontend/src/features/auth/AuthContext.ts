import { createContext } from 'react';
import type { LoginRequest, MeResponse } from './types';

export type AuthState =
  | { status: 'initializing' }
  | { status: 'authenticated'; user: MeResponse }
  | { status: 'unauthenticated' };

export interface AuthContextValue {
  state: AuthState;
  login: (request: LoginRequest) => Promise<void>;
  logout: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
