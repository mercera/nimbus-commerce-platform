import { useState, type FormEvent } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import styles from './LoginPage.module.css';
import { useAuth } from './useAuth';
import { validateEmail, validateRequired } from './validation';
import { ApiError } from '../../lib/api/errors';
import { getSafeRedirectPath } from '../../routes/safeRedirect';
import { TextField } from '../../components/TextField';
import { Button } from '../../components/Button';
import { FormMessages } from '../../components/FormMessages';

interface FieldErrors {
  email?: string;
  password?: string;
}

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const successMessage =
    typeof (location.state as { message?: string } | null)?.message === 'string'
      ? (location.state as { message: string }).message
      : null;
  const redirectTo =
    getSafeRedirectPath((location.state as { from?: string } | null)?.from) ?? '/dashboard';

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const [apiMessages, setApiMessages] = useState<string[]>([]);
  const [submitting, setSubmitting] = useState(false);

  function validate(): boolean {
    const errors: FieldErrors = {};
    const emailError = validateEmail(email);
    if (emailError) errors.email = emailError;
    const passwordError = validateRequired(password, 'Password');
    if (passwordError) errors.password = passwordError;
    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setApiMessages([]);
    if (!validate()) return;

    setSubmitting(true);
    try {
      await login({ email, password });
      navigate(redirectTo, { replace: true });
    } catch (error) {
      if (error instanceof ApiError) {
        setApiMessages(error.messages);
      } else {
        setApiMessages(['Something went wrong. Please try again.']);
      }
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form className={styles.form} onSubmit={handleSubmit} noValidate>
      <h1 className={styles.heading}>Sign in</h1>

      {successMessage && <FormMessages messages={[successMessage]} variant="success" />}
      <FormMessages messages={apiMessages} variant="error" />

      <TextField
        label="Email"
        type="email"
        autoComplete="email"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        error={fieldErrors.email}
        disabled={submitting}
        required
      />
      <TextField
        label="Password"
        type="password"
        autoComplete="current-password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        error={fieldErrors.password}
        disabled={submitting}
        required
      />

      <Button type="submit" loading={submitting}>
        Sign in
      </Button>

      <p className={styles.footer}>
        Don&apos;t have an account? <Link to="/register">Create one</Link>
      </p>
    </form>
  );
}
