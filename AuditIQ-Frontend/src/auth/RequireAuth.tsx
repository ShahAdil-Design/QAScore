import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useIsAuthenticated, useMsal } from '@azure/msal-react'
import { InteractionStatus } from '@azure/msal-browser'
import { useAuth } from './authStore'
import { isSsoConfigured } from './msalConfig'

export function RequireAuth({ children }: { children: ReactNode }) {
  // Two independent auth sources, never both active: MSAL once real SSO config exists, the
  // dev-mode in-memory token store otherwise (see msalConfig.ts / LoginPage.tsx).
  const devAuthenticated = useAuth((s) => s.isAuthenticated)
  const msalAuthenticated = useIsAuthenticated()
  const { inProgress } = useMsal()
  const isAuthenticated = isSsoConfigured ? msalAuthenticated : devAuthenticated
  const location = useLocation()

  // While MSAL is still processing the redirect back from Entra ID (inProgress !==
  // InteractionStatus.None), rendering <Navigate replace> here would call
  // history.replaceState and strip the #code=... hash out of the URL before MSAL's own async
  // handleRedirectPromise() gets a chance to read it — silently breaking every redirect login.
  // Render nothing (briefly) instead of redirecting until that settles.
  if (isSsoConfigured && inProgress !== InteractionStatus.None) {
    return null
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  return children
}
