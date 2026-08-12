import { Link } from 'react-router-dom';

export function NotFoundPage() {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: '100vh', gap: '1rem' }}>
      <h1>404</h1>
      <p>Page not found.</p>
      <Link to="/dashboard">Go to Dashboard</Link>
    </div>
  );
}
