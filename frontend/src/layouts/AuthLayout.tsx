import { Outlet } from 'react-router-dom';
import styles from './AuthLayout.module.css';

export function AuthLayout() {
  return (
    <div className={styles.wrapper}>
      <div className={styles.card}>
        <div className={styles.brand}>
          <span className={styles.brandName}>Nimbus Commerce</span>
          <span className={styles.brandTagline}>Cloud-native commerce platform</span>
        </div>
        <Outlet />
      </div>
    </div>
  );
}
