let accessToken: string | null = null;
let onSessionExpired: (() => void) | null = null;

export const tokenStore = {
  get: (): string | null => accessToken,
  set: (token: string): void => {
    accessToken = token;
  },
  clear: (): void => {
    accessToken = null;
  },
  setOnSessionExpired: (callback: () => void): void => {
    onSessionExpired = callback;
  },
  notifySessionExpired: (): void => {
    onSessionExpired?.();
  },
};
