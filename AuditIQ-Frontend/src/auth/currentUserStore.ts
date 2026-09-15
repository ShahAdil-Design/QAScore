import { create } from 'zustand'
import type { UserDetail } from '@/api/users'

/**
 * The real AuditIQ identity resolved from the backend's /users/me (which itself resolves role
 * from AuditIqRoleClaimsTransformation — see backend Program.cs). Kept separate from authStore
 * (which only holds the raw token) to avoid a circular import: authStore is used by the API
 * client itself, so it can't depend on an API call.
 *
 * Null means "unresolved" — either nobody's logged in, or they came in through the legacy plain
 * dev-login button (no email claim, so /me can't resolve anything). UI that gates on role should
 * treat null as "show everything", not "show nothing", to keep that legacy path fully working.
 */
interface CurrentUserStore {
  me: UserDetail | null
  setMe: (me: UserDetail | null) => void
}

export const useCurrentUser = create<CurrentUserStore>((set) => ({
  me: null,
  setMe: (me) => set({ me }),
}))
