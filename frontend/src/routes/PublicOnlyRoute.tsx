import type { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../features/auth/useAuth';

interface PublicOnlyRouteProps {
  children: ReactNode;
}

/** Redirects an already-authenticated user away from /login and /register. */
export function PublicOnlyRoute({ children }: PublicOnlyRouteProps) {
  const { state } = useAuth();

  if (state.status === 'authenticated') {
    return <Navigate to="/dashboard" replace />;
  }

  return <>{children}</>;
}
