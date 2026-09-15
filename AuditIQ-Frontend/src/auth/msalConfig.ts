import type { Configuration } from '@azure/msal-browser'
import { LogLevel } from '@azure/msal-browser'
import type { RuntimeConfig } from '@/config/runtimeConfig'

// Settled once at startup (msalInstance.ts's init sequence, gated in main.tsx before anything
// renders) and never reassigned afterward — plain `let` exports are safe here precisely because
// nothing ever reads them before that gate resolves.
export let isSsoConfigured = false
export let loginRequest: { scopes: string[] } = { scopes: [] }

export function applyRuntimeConfig(runtimeConfig: RuntimeConfig): Configuration {
  const { clientId, tenantId, redirectUri, requiredScopes } = runtimeConfig.azureAuth

  isSsoConfigured = Boolean(clientId && tenantId)
  // requiredScopes arrives as a single (optionally comma-separated) string — see the comment on
  // RuntimeConfig.azureAuth.requiredScopes in runtimeConfig.ts for why this isn't a JSON array.
  loginRequest = { scopes: requiredScopes.split(',').map((s) => s.trim()).filter(Boolean) }

  return {
    auth: {
      clientId,
      authority: `https://login.microsoftonline.com/${tenantId}`,
      redirectUri: redirectUri || window.location.origin,
      postLogoutRedirectUri: redirectUri || window.location.origin,
    },
    cache: {
      // localStorage (not sessionStorage): shared across every tab of the same origin, so
      // opening AuditIQ in a new tab while already signed in elsewhere picks up the existing
      // session immediately instead of forcing another interactive Microsoft sign-in per tab —
      // sessionStorage's per-tab isolation was causing exactly that. Session now persists across
      // a full browser restart too, until the token/refresh token actually expires or the user
      // explicitly signs out — Microsoft's own recommended MSAL config for this scenario.
      cacheLocation: 'localStorage',
    },
    system: {
      loggerOptions: {
        loggerCallback: (level, message, containsPii) => {
          if (containsPii) return
          if (level === LogLevel.Error) console.error('[MSAL]', message)
        },
      },
    },
  }
}
