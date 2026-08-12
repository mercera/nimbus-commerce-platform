import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import styles from './AppLayout.module.css';
import { useAuth } from '../features/auth/useAuth';
import { Button } from '../components/Button';

export function AppLayout() {
  const { state, logout } = useAuth();
  const navigate = useNavigate();

  const user = state.status === 'authenticated' ? state.user : null;

  async function handleLogout() {
    await logout();
    navigate('/login', { replace: true });
  }

  return (
    <div className={styles.shell}>
      <header className={styles.header}>
        <span className={styles.brand}>Nimbus Commerce</span>
        {user && (
          <div className={styles.userInfo}>
            <span className={styles.userName}>
              {user.firstName} {user.lastName}
            </span>
            <Button type="button" variant="secondary" onClick={handleLogout}>
              Logout
            </Button>
          </div>
        )}
      </header>

      <div className={styles.body}>
        <nav className={styles.nav}>
          <NavLink
            to="/dashboard"
            className={({ isActive }) =>
              [styles.navItem, isActive ? styles.navItemActive : ''].filter(Boolean).join(' ')
            }
          >
            Dashboard
          </NavLink>
          <span className={styles.navItemDisabled} aria-disabled="true">
            Products <span className={styles.comingSoon}>Soon</span>
          </span>
          <span className={styles.navItemDisabled} aria-disabled="true">
            Orders <span className={styles.comingSoon}>Soon</span>
          </span>
        </nav>

        <main className={styles.main}>
          <Outlet />
        </main>
      </div>
    </div>
  );
}
