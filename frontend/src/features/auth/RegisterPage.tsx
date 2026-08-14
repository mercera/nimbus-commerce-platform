import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import styles from './RegisterPage.module.css';
import * as authApi from './api';
import { getPasswordRequirements, validateEmail, validateRequired } from './validation';
import { ApiError } from '../../lib/api/errors';
import { TextField } from '../../components/TextField';
import { Button } from '../../components/Button';
import { FormMessages } from '../../components/FormMessages';

interface FieldErrors {
  email?: string;
  password?: string;
  confirmPassword?: string;
  firstName?: string;
  lastName?: string;
}

function fieldMessages(fieldErrors: Record<string, string[]> | undefined, name: string): string[] {
  if (!fieldErrors) return [];
  const key = Object.keys(fieldErrors).find((k) => k.toLowerCase() === name.toLowerCase());
  return key ? fieldErrors[key] : [];
}

export function RegisterPage() {
  const navigate = useNavigate();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');

  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const [apiMessages, setApiMessages] = useState<string[]>([]);
  const [submitting, setSubmitting] = useState(false);
  const [passwordTouched, setPasswordTouched] = useState(false);

  const passwordRequirements = getPasswordRequirements(password);

  function validate(): boolean {
    const errors: FieldErrors = {};

    const emailError = validateEmail(email);
    if (emailError) errors.email = emailError;

    const unmetRequirements = passwordRequirements.filter((r) => !r.met);
    if (unmetRequirements.length > 0) {
      errors.password = 'Password does not meet the requirements below.';
    }

    if (password !== confirmPassword) {
      errors.confirmPassword = 'Passwords do not match.';
    }

    const firstNameError = validateRequired(firstName, 'First name');
    if (firstNameError) errors.firstName = firstNameError;

    const lastNameError = validateRequired(lastName, 'Last name');
    if (lastNameError) errors.lastName = lastNameError;

    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setApiMessages([]);
    if (!validate()) return;

    setSubmitting(true);
    try {
      await authApi.register({ email, password, confirmPassword, firstName, lastName });
      navigate('/login', {
        replace: true,
        state: { message: 'Account created — please sign in.' },
      });
    } catch (error) {
      if (error instanceof ApiError) {
        if (error.fieldErrors) {
          setFieldErrors({
            email: fieldMessages(error.fieldErrors, 'Email')[0],
            password: fieldMessages(error.fieldErrors, 'Password')[0],
            confirmPassword: fieldMessages(error.fieldErrors, 'ConfirmPassword')[0],
            firstName: fieldMessages(error.fieldErrors, 'FirstName')[0],
            lastName: fieldMessages(error.fieldErrors, 'LastName')[0],
          });
        } else {
          setApiMessages(error.messages);
        }
      } else {
        setApiMessages(['Something went wrong. Please try again.']);
      }
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form className={styles.form} onSubmit={handleSubmit} noValidate>
      <h1 className={styles.heading}>Create your account</h1>

      <FormMessages messages={apiMessages} variant="error" />

      <div className={styles.row}>
        <TextField
          label="First name"
          autoComplete="given-name"
          value={firstName}
          onChange={(e) => setFirstName(e.target.value)}
          error={fieldErrors.firstName}
          disabled={submitting}
          required
        />
        <TextField
          label="Last name"
          autoComplete="family-name"
          value={lastName}
          onChange={(e) => setLastName(e.target.value)}
          error={fieldErrors.lastName}
          disabled={submitting}
          required
        />
      </div>

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
        autoComplete="new-password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        onFocus={() => setPasswordTouched(true)}
        error={fieldErrors.password}
        disabled={submitting}
        required
      />
      {passwordTouched && (
        <ul className={styles.requirements}>
          {passwordRequirements.map((requirement) => (
            <li
              key={requirement.label}
              className={requirement.met ? styles.requirementMet : undefined}
            >
              {requirement.met ? '✓' : '·'} {requirement.label}
            </li>
          ))}
        </ul>
      )}

      <TextField
        label="Confirm password"
        type="password"
        autoComplete="new-password"
        value={confirmPassword}
        onChange={(e) => setConfirmPassword(e.target.value)}
        error={fieldErrors.confirmPassword}
        disabled={submitting}
        required
      />

      <Button type="submit" loading={submitting}>
        Create account
      </Button>

      <p className={styles.footer}>
        Already have an account? <Link to="/login">Sign in</Link>
      </p>
    </form>
  );
}
