import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../features/auth/useAuth';

interface ProtectedRouteProps {
  children: ReactNode;
}

/**
 * AuthProvider renders a full-page loader while status === 'initializing',
 * so this component never mounts in that state — no redirect branch needed
 * for it. Only 'unauthenticated' redirects; 'authenticated' renders through.
 */
export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const { state } = useAuth();
  const location = useLocation();

  if (state.status === 'unauthenticated') {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }

  return <>{children}</>;
}
