import { useQuery } from '@tanstack/react-query'
import { screenPermissionsApi } from '@/api/screenPermissions'
import { useCurrentUser } from './currentUserStore'

/**
 * Screen-visibility only — mirrors the backend's ScreenPermissions table (seeded to exactly
 * match the nav's old hardcoded adminOnly flags). This is NOT the real security boundary; the
 * backend's [Authorize(Policy = ...)] checks are. A role could in principle be granted
 * visibility here while its underlying API calls still 403 — that inconsistency is a known,
 * accepted tradeoff of doing screen-visibility management before also making it the backend's
 * source of truth for authorization.
 */
export function useScreenAccess() {
  const me = useCurrentUser((s) => s.me)
  const permissionsQuery = useQuery({
    queryKey: ['screen-permissions'],
    queryFn: () => screenPermissionsApi.list(),
    staleTime: 5 * 60 * 1000,
  })

  function canSeeScreen(screenKey: string): boolean {
    // Unresolved identity (legacy dev login, no email claim) keeps full access — same
    // convention as everywhere else this session gates on role.
    if (!me) return true
    // While the matrix is still loading, don't flash the nav to empty — the backend remains
    // the real enforcement point regardless of what's shown here.
    if (!permissionsQuery.data) return true

    const row = permissionsQuery.data.find((p) => p.role === me.role && p.screenKey === screenKey)
    return row?.isVisible ?? false
  }

  return { canSeeScreen, isLoading: permissionsQuery.isLoading, permissions: permissionsQuery.data ?? [] }
}
