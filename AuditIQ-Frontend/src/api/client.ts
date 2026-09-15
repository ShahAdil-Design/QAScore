import type { ProblemDetails } from './types'
import { useAuth } from '@/auth/authStore'
import { useCurrentUser } from '@/auth/currentUserStore'
import { getAccessToken } from '@/auth/getAccessToken'
import { loadRuntimeConfig } from '@/config/runtimeConfig'

export class ApiError extends Error {
  readonly status: number
  readonly problem: ProblemDetails | null

  constructor(status: number, problem: ProblemDetails | null, fallback: string) {
    super(problem?.title ?? problem?.detail ?? fallback)
    this.name = 'ApiError'
    this.status = status
    this.problem = problem
  }
}

/** Thin typed fetch wrapper. Attaches the bearer token, throws ApiError on
 * non-2xx responses so TanStack Query's error handling can surface it. */
export async function apiRequest<T>(path: string, init?: RequestInit): Promise<T> {
  const [token, runtimeConfig] = await Promise.all([getAccessToken(), loadRuntimeConfig()])

  const res = await fetch(`${runtimeConfig.api.baseUrl}${path}`, {
    ...init,
    headers: {
      Accept: 'application/json',
      ...(init?.body ? { 'Content-Type': 'application/json' } : {}),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(init?.headers ?? {}),
    },
  })

  if (res.status === 401) {
    useAuth.getState().logout()
    useCurrentUser.getState().setMe(null)
    const back = encodeURIComponent(window.location.pathname + window.location.search)
    window.location.href = `/login?redirectUri=${back}`
    return new Promise<T>(() => {}) // never resolves; navigation is happening
  }

  if (!res.ok) {
    let problem: ProblemDetails | null = null
    try {
      problem = await res.json()
    } catch {
      /* not JSON */
    }
    throw new ApiError(res.status, problem, `${res.status} ${res.statusText}`)
  }

  if (res.status === 204) return undefined as T
  return (await res.json()) as T
}

export const api = {
  health: () => apiRequest<string>('/health'),
}
