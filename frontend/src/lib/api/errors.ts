export class ApiError extends Error {
  readonly status: number;
  readonly messages: string[];
  readonly fieldErrors?: Record<string, string[]>;

  constructor(status: number, messages: string[], fieldErrors?: Record<string, string[]>) {
    super(messages[0] ?? 'Request failed.');
    this.name = 'ApiError';
    this.status = status;
    this.messages = messages.length > 0 ? messages : ['Request failed.'];
    this.fieldErrors = fieldErrors;
  }
}

function statusFallbackMessage(status: number): string {
  if (status === 401) return 'You are not signed in.';
  if (status === 403) return 'You do not have permission to do that.';
  if (status === 404) return 'The requested resource was not found.';
  if (status >= 500) return 'Something went wrong on the server. Please try again.';
  return 'Request failed.';
}

/**
 * Normalizes the four response shapes the backend actually produces:
 * ValidationProblemDetails, a bare string[] (Register/Identity errors),
 * { message }, and an empty body (JWT middleware 401s never send JSON).
 */
export async function toApiError(response: Response): Promise<ApiError> {
  const status = response.status;

  let text: string;
  try {
    text = await response.text();
  } catch {
    return new ApiError(status, [statusFallbackMessage(status)]);
  }

  if (!text) {
    return new ApiError(status, [statusFallbackMessage(status)]);
  }

  let body: unknown;
  try {
    body = JSON.parse(text);
  } catch {
    return new ApiError(status, [statusFallbackMessage(status)]);
  }

  // Bare string[] — Register's IdentityOperationResult.Errors
  if (Array.isArray(body)) {
    const messages = body.filter((item): item is string => typeof item === 'string');
    return new ApiError(status, messages.length > 0 ? messages : [statusFallbackMessage(status)]);
  }

  if (typeof body === 'object' && body !== null) {
    const record = body as Record<string, unknown>;

    // ValidationProblemDetails — { errors: { "Field": ["msg", ...] } }
    if ('errors' in record && typeof record.errors === 'object' && record.errors !== null) {
      const fieldErrors: Record<string, string[]> = {};
      const messages: string[] = [];
      for (const [field, value] of Object.entries(record.errors as Record<string, unknown>)) {
        if (Array.isArray(value)) {
          const fieldMessages = value.filter((item): item is string => typeof item === 'string');
          fieldErrors[field] = fieldMessages;
          messages.push(...fieldMessages);
        }
      }
      return new ApiError(status, messages, fieldErrors);
    }

    // { message } — Login/Refresh/Me failure shape
    if (typeof record.message === 'string') {
      return new ApiError(status, [record.message]);
    }

    // { title } — ProblemDetails without field errors
    if (typeof record.title === 'string') {
      return new ApiError(status, [record.title]);
    }
  }

  return new ApiError(status, [statusFallbackMessage(status)]);
}

export function toNetworkError(): ApiError {
  return new ApiError(0, ['Unable to reach the server. Check your connection and try again.']);
}
