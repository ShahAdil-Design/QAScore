import { useEffect, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { Loader2, ShieldAlert, ShieldCheck, ShieldQuestion, User, Users } from 'lucide-react'
import { useMsal, useIsAuthenticated } from '@azure/msal-react'
import { InteractionStatus } from '@azure/msal-browser'
import OakbrookCircle from '@/icons/OakbrookCircle'
import { useAuth } from '@/auth/authStore'
import { useCurrentUser } from '@/auth/currentUserStore'
import { isSsoConfigured, loginRequest } from '@/auth/msalConfig'
import { REDIRECT_ATTEMPTED_KEY } from '@/auth/getAccessToken'
import { usersApi } from '@/api/users'
import { ApiError } from '@/api/client'
import { Button } from '@/components/ui/Button'

/**
 * Real SSO (MSAL, isSsoConfigured) once DevOps hands over the App Registration details
 * (Section 13) — until then this falls back to the dev-only "Continue as ..." buttons behind
 * VITE_ENABLE_DEV_LOGIN, so every environment without real Azure config keeps working exactly
 * as before.
 */
export function LoginPage() {
  const setToken = useAuth((s) => s.setToken)
  const setMe = useCurrentUser((s) => s.setMe)
  const navigate = useNavigate()
  const location = useLocation()
  const from = (location.state as { from?: Location })?.from?.pathname ?? '/'

  const { instance, inProgress } = useMsal()
  const msalAuthenticated = useIsAuthenticated()

  // A 403 here means the caller authenticated but has no matching (active) row in AuditIQ's own
  // Users table — expected for anyone before an Admin provisions them via the Staff page. Shown
  // as a dedicated message rather than silently letting an identity-less session into the app.
  const [notProvisioned, setNotProvisioned] = useState(false)
  // Anything other than "not provisioned" while resolving identity post-redirect — most notably
  // getAccessToken's redirect-loop guard tripping — shown directly rather than leaving the user
  // staring at a login button that silently does nothing.
  const [ssoError, setSsoError] = useState<string | null>(null)
  // True for the window between "Entra ID redirect completed" and "AuditIQ identity resolved" —
  // usersApi.getMe() can take a couple of seconds (real backend round trip, not instant), and
  // without this the plain sign-in button rendered underneath made it look like nothing had
  // happened / the login had silently failed, right after the user just finished signing in.
  const [resolvingSso, setResolvingSso] = useState(false)

  async function continueAs(token: string) {
    setNotProvisioned(false)
    setToken(token)
    try {
      const me = await usersApi.getMe()
      setMe(me)
      navigate(from, { replace: true })
    } catch (err) {
      useAuth.getState().logout()
      setMe(null)
      setNotProvisioned(err instanceof ApiError && err.status === 403)
    }
  }

  // Once MSAL reports a signed-in account (the redirect back from Entra ID completed), resolve
  // the real AuditIQ user the same way the dev flow does, then leave /login.
  useEffect(() => {
    if (!isSsoConfigured || !msalAuthenticated) return
    let cancelled = false
    setResolvingSso(true)
    void (async () => {
      try {
        const me = await usersApi.getMe()
        if (cancelled) return
        setMe(me)
        navigate(from, { replace: true })
      } catch (err) {
        if (cancelled) return
        setMe(null)
        const isNotProvisioned = err instanceof ApiError && err.status === 403
        setNotProvisioned(isNotProvisioned)
        setSsoError(isNotProvisioned ? null : err instanceof Error ? err.message : 'Sign-in failed unexpectedly.')
      } finally {
        if (!cancelled) setResolvingSso(false)
      }
    })()
    return () => { cancelled = true }
  }, [msalAuthenticated, from, navigate, setMe])

  function signOutAndRetry() {
    setNotProvisioned(false)
    if (isSsoConfigured) void instance.logoutRedirect()
  }

  // Covers two windows, not just one: MSAL still processing the redirect on the very first
  // render (inProgress !== None — before useIsAuthenticated() even has an answer yet) and our
  // own getMe() call once it does (resolvingSso). Without the inProgress half, the plain sign-in
  // screen flashes for a frame or two before this one takes over, right as the user lands back
  // from Entra ID — looks like a glitch even though nothing is actually wrong.
  if (isSsoConfigured && (inProgress !== InteractionStatus.None || resolvingSso)) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-background text-foreground">
        <div className="rounded-2xl border border-border bg-card p-8 max-w-sm w-full text-center shadow-sm">
          <div className="mx-auto flex size-10 items-center justify-center rounded-xl bg-primary text-primary-foreground">
            <Loader2 className="size-5 animate-spin" />
          </div>
          <h1 className="mt-4 text-xl font-semibold">Signing you in...</h1>
          <p className="mt-2 text-muted-foreground text-sm">Just a moment while we finish setting up your session.</p>
        </div>
      </div>
    )
  }

  if (notProvisioned) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-background text-foreground">
        <div className="rounded-2xl border border-border bg-card p-8 max-w-sm w-full text-center shadow-sm">
          <div className="mx-auto flex size-10 items-center justify-center rounded-xl bg-status-fail-bg text-status-fail">
            <ShieldAlert className="size-5" />
          </div>
          <h1 className="mt-4 text-xl font-semibold">Access not set up yet</h1>
          <p className="mt-2 text-muted-foreground text-sm">
            You signed in successfully, but your account hasn't been added to AuditIQ yet. Ask an
            AuditIQ Admin to add you on the Staff page, then try again.
          </p>
          <Button className="mt-4 w-full" variant="secondary" onClick={signOutAndRetry}>
            {isSsoConfigured ? 'Sign out' : 'Back to sign in'}
          </Button>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-background text-foreground">
      <div className="rounded-2xl border border-border bg-card p-8 max-w-sm w-full text-center shadow-sm">
        <div className="mx-auto flex size-10 items-center justify-center">
          <OakbrookCircle className="size-9" />
        </div>
        <h1 className="mt-4 text-xl font-semibold">Sign in to AuditIQ</h1>

        {isSsoConfigured ? (
          <>
            <p className="mt-2 text-muted-foreground text-sm">Sign in with your organization account.</p>
            <Button
              className="mt-4 w-full"
              onClick={() => {
                setSsoError(null)
                sessionStorage.removeItem(REDIRECT_ATTEMPTED_KEY)
                void instance.loginRedirect(loginRequest)
              }}
            >
              <ShieldCheck className="size-4" /> Sign in with Microsoft
            </Button>
            {ssoError && <p className="mt-3 text-sm text-status-fail">{ssoError}</p>}
          </>
        ) : (
          <p className="mt-2 text-muted-foreground text-sm">
            SSO sign-in is not yet wired up (blocked on Section 13's org SSO client details).
          </p>
        )}

        {(import.meta.env.DEV || import.meta.env.VITE_ENABLE_DEV_LOGIN === 'true') && (
          <div className="mt-4 flex flex-col gap-2">
            <Button className="w-full" onClick={() => continueAs('dev:demo.admin@auditiq.local')}>
              <ShieldCheck className="size-4" /> Continue as Admin (dev)
            </Button>
            <Button variant="secondary" className="w-full" onClick={() => continueAs('dev:nikit.ghosh@oakbrookfinance.com')}>
              <User className="size-4" /> Continue as Agent (dev)
            </Button>
            <Button variant="secondary" className="w-full" onClick={() => continueAs('dev:shraddha.hire@oakbrookfinance.com')}>
              <Users className="size-4" /> Continue as Supervisor (dev)
            </Button>
            <Button variant="ghost" className="w-full" onClick={() => continueAs('dev-placeholder-token')}>
              <ShieldQuestion className="size-4" /> Continue (dev only, unprovisioned)
            </Button>
          </div>
        )}
      </div>
    </div>
  )
}
