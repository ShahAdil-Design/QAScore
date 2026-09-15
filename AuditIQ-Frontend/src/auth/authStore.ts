import { create } from 'zustand'

/**
 * Placeholder token store. AuditIQ's backend expects a JWT bearer token
 * validated against the org SSO's OIDC endpoint (see backend Sso:Authority /
 * Sso:Audience config) — but the SSO client details (Authority, client ID,
 * redirect URIs) are still TBD (master doc Section 13). Wire the real OIDC
 * client (e.g. oidc-client-ts / MSAL, whichever the org's convention turns
 * out to be) into `login`/`logout` once that's confirmed; until then this
 * only tracks an in-memory token so the rest of the app (route guards, the
 * API client's Authorization header) can be built against a stable shape.
 */
interface AuthStore {
  token: string | null
  isAuthenticated: boolean
  setToken: (token: string) => void
  logout: () => void
}

export const useAuth = create<AuthStore>((set) => ({
  token: null,
  isAuthenticated: false,
  setToken: (token) => set({ token, isAuthenticated: true }),
  logout: () => set({ token: null, isAuthenticated: false }),
}))
