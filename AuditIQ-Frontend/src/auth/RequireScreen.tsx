import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useCurrentUser } from './currentUserStore'
import { useScreenAccess } from './useScreenAccess'
import { ScreenKeys } from '@/api/screenPermissions'

// Longest/most-specific prefixes first — a path is matched against these in order, so a
// sub-route (e.g. /calibration/items/:itemId) still resolves to its parent screen's key.
const routeScreens: { prefix: string; screenKey: string }[] = [
  { prefix: '/score', screenKey: ScreenKeys.Score },
  { prefix: '/review', screenKey: ScreenKeys.Review },
  { prefix: '/calibration', screenKey: ScreenKeys.Calibration },
  { prefix: '/reports', screenKey: ScreenKeys.Reports },
  { prefix: '/scorecards', screenKey: ScreenKeys.Scorecards },
  { prefix: '/staff', screenKey: ScreenKeys.Staff },
  { prefix: '/', screenKey: ScreenKeys.Dashboard },
]

/** Real route enforcement — closes the gap where a hidden nav link was still reachable by typing
 * its URL directly. Screen-visibility only, same caveat as everywhere else this session: the
 * backend's [Authorize(Policy = ...)] checks remain the actual security boundary. */
export function RequireScreen({ children }: { children: ReactNode }) {
  const location = useLocation()
  const me = useCurrentUser((s) => s.me)
  const { canSeeScreen } = useScreenAccess()

  if (location.pathname.startsWith('/settings')) {
    const isAdmin = !me || me.role === 'Admin'
    return isAdmin ? children : <Navigate to="/" replace />
  }

  const match = routeScreens.find((r) => location.pathname.startsWith(r.prefix))
  if (match && !canSeeScreen(match.screenKey) && location.pathname !== '/') {
    return <Navigate to="/" replace />
  }

  return children
}
