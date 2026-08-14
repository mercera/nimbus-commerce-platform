/**
 * Only accepts a same-origin absolute path ("/dashboard"), never a
 * protocol-relative or external URL ("//evil.com", "https://evil.com") —
 * cheap protection against an open redirect via the return-to location.
 */
export function getSafeRedirectPath(path: unknown): string | null {
  if (typeof path !== 'string') return null;
  if (!path.startsWith('/') || path.startsWith('//')) return null;
  return path;
}
