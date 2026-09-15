import { InteractionRequiredAuthError } from '@azure/msal-browser'
import { isSsoConfigured, loginRequest } from './msalConfig'
import { msalInstance } from './msalInstance'
import { useAuth } from './authStore'

// Guards against a redirect loop: if silent token acquisition needs interaction again within
// the same tab session shortly after we already went through an interactive redirect once
// (e.g. third-party cookies blocking the silent-iframe renewal path, or a consent/scope issue
// specific to this account), redirecting again just repeats the exact same failure forever —
// each cycle looking identical from the outside (sign in -> bounced straight back to /login).
// One automatic redirect per tab session is enough; past that, surface an error instead.
export const REDIRECT_ATTEMPTED_KEY = 'auditiq:msal-redirect-attempted'

/** Single token source for apiRequest (client.ts) — real MSAL silent acquisition once SSO is
 * configured, the dev-mode in-memory token otherwise. Never both at once. */
export async function getAccessToken(): Promise<string | null> {
  if (!isSsoConfigured) return useAuth.getState().token

  // getActiveAccount() can still be null for a brief window right after a redirect-login
  // completes — it's only set by our own LOGIN_SUCCESS callback (msalInstance.ts), which can
  // run on a later tick than useIsAuthenticated() flipping true (that just checks "any account
  // exists in the cache", a different, earlier-populated piece of MSAL state). Falling back to
  // the first cached account avoids sending the very first post-login API call with no
  // Authorization header at all — previously this made GET /users/me look like an unauthenticated
  // call (surfacing as "Access not set up yet") until a refresh let the active account settle.
  const account = msalInstance.getActiveAccount() ?? msalInstance.getAllAccounts()[0]
  if (!account) return null
  if (!msalInstance.getActiveAccount()) msalInstance.setActiveAccount(account)

  try {
    const result = await msalInstance.acquireTokenSilent({ ...loginRequest, account })
    sessionStorage.removeItem(REDIRECT_ATTEMPTED_KEY)
    return result.accessToken
  } catch (error) {
    if (error instanceof InteractionRequiredAuthError) {
      if (sessionStorage.getItem(REDIRECT_ATTEMPTED_KEY)) {
        throw new Error(
          'Sign-in keeps requiring another interactive step (often caused by the browser blocking ' +
          'third-party cookies, which breaks silent token renewal). Check the browser console for the ' +
          '[MSAL] error logged just before this, and see if third-party cookies are blocked for this site.',
        )
      }
      sessionStorage.setItem(REDIRECT_ATTEMPTED_KEY, '1')
      // Redirects away to re-authenticate interactively — the in-flight request this token was
      // for never resolves, which is expected: the page is navigating away.
      await msalInstance.acquireTokenRedirect(loginRequest)
      return null
    }
    throw error
  }
}
