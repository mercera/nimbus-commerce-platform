export interface PasswordRequirement {
  label: string;
  met: boolean;
}

/**
 * Mirrors the server-side password policy: RegisterRequest only declares
 * [MinLength(8)], but AddIdentityCore in Infrastructure/DependencyInjection.cs
 * sets RequiredLength = 8 and leaves every other ASP.NET Core Identity default
 * on (digit, lowercase, uppercase, non-alphanumeric all required). A password
 * that passes DataAnnotations alone still fails at the server otherwise.
 */
export function getPasswordRequirements(password: string): PasswordRequirement[] {
  return [
    { label: 'At least 8 characters', met: password.length >= 8 },
    { label: 'At least one digit', met: /[0-9]/.test(password) },
    { label: 'At least one lowercase letter', met: /[a-z]/.test(password) },
    { label: 'At least one uppercase letter', met: /[A-Z]/.test(password) },
    { label: 'At least one symbol', met: /[^a-zA-Z0-9]/.test(password) },
  ];
}

export function validatePassword(password: string): string[] {
  return getPasswordRequirements(password)
    .filter((requirement) => !requirement.met)
    .map((requirement) => requirement.label);
}

export function validateEmail(email: string): string | null {
  if (!email.trim()) return 'Email is required.';
  // Deliberately loose — the server validates the real rule; this only
  // catches obviously-empty or obviously-malformed input before submit.
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return 'Enter a valid email address.';
  return null;
}

export function validateRequired(value: string, fieldLabel: string): string | null {
  return value.trim() ? null : `${fieldLabel} is required.`;
}
