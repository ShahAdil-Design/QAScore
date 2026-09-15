export interface RuntimeConfig {
  api: {
    // Absolute backend URL (e.g. "https://auditiq.api.oak.d01.o6k.xyz"), injected by Octopus —
    // nginx serves only the static SPA and has no /api proxy, so a relative path here would fall
    // through nginx's SPA catch-all (`try_files ... /index.html`) and return HTML instead of JSON.
    // Left "" for local dev, where Vite's own dev-server proxy (vite.config.ts) forwards relative
    // /api calls to the local backend instead.
    baseUrl: string
  }
  azureAuth: {
    clientId: string
    tenantId: string
    redirectUri: string
    // A single (optionally comma-separated) string, not a JSON array — Octopus's JSON
    // Configuration Variables step can't cleanly substitute a #{Token} nested inside literal
    // array brackets (it tries to re-parse the whole array as JSON first), but substituting a
    // scalar string value is trivial. Split into MSAL's string[] scopes in msalConfig.ts.
    requiredScopes: string
  }
}

const emptyConfig: RuntimeConfig = {
  api: { baseUrl: '' },
  azureAuth: { clientId: '', tenantId: '', redirectUri: '', requiredScopes: '' },
}

let cached: RuntimeConfig | null = null

/**
 * Fetches /config/appsettings.config.json at runtime (never at build time) — the same file
 * Octopus's "Substitute Variables in Templates" step fills in per-environment on the deployed
 * chart (charts/auditiq-frontend/appsettings.config.json), served by nginx's dedicated no-cache
 * location block for this exact path. In local dev, Vite serves the committed placeholder copy
 * under public/config/ the same way. Never hardcode real tenant/client values into either copy
 * of this file — they're injected at deploy time, not committed.
 */
export async function loadRuntimeConfig(): Promise<RuntimeConfig> {
  if (cached) return cached

  try {
    const res = await fetch('/config/appsettings.config.json', { cache: 'no-store' })
    if (!res.ok) throw new Error(`HTTP ${res.status}`)
    const data = await res.json()
    cached = {
      api: {
        baseUrl: data?.api?.baseUrl ?? '',
      },
      azureAuth: {
        clientId: data?.azureAuth?.clientId ?? '',
        tenantId: data?.azureAuth?.tenantId ?? '',
        redirectUri: data?.azureAuth?.redirectUri ?? '',
        requiredScopes: typeof data?.azureAuth?.requiredScopes === 'string' ? data.azureAuth.requiredScopes : '',
      },
    }
  } catch {
    // Missing/unreachable (e.g. this file 404s in some environment) falls back to "SSO not
    // configured" rather than crashing app startup — same fail-safe the dev-login flow relies on.
    cached = emptyConfig
  }

  return cached
}
