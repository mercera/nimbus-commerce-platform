import styles from './DashboardPage.module.css';
import { useAuth } from '../auth/useAuth';

export function DashboardPage() {
  const { state } = useAuth();
  const user = state.status === 'authenticated' ? state.user : null;

  return (
    <div className={styles.wrapper}>
      <h1>Dashboard</h1>
      <p className={styles.subtitle}>
        This is the authenticated application shell. Products and Orders land in future
        milestones.
      </p>

      {user && (
        <div className={styles.card}>
          <dl>
            <dt>Name</dt>
            <dd>
              {user.firstName} {user.lastName}
            </dd>
            <dt>Email</dt>
            <dd>{user.email}</dd>
            <dt>User ID</dt>
            <dd>{user.id}</dd>
            <dt>Roles</dt>
            <dd>{user.roles.length > 0 ? user.roles.join(', ') : 'None assigned'}</dd>
          </dl>
        </div>
      )}
    </div>
  );
}
