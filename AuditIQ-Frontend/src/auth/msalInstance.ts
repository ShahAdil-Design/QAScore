import { PublicClientApplication, EventType, type AccountInfo } from '@azure/msal-browser'
import { applyRuntimeConfig } from './msalConfig'
import { loadRuntimeConfig } from '@/config/runtimeConfig'

// Assigned inside msalInitialized below, before main.tsx ever renders anything (it awaits this
// promise first) — every consumer that imports msalInstance only ever runs after that gate, so
// this is safe despite not being initialized at module-evaluation time.
export let msalInstance: PublicClientApplication

export const msalInitialized = (async () => {
  const runtimeConfig = await loadRuntimeConfig()
  const config = applyRuntimeConfig(runtimeConfig)

  msalInstance = new PublicClientApplication(config)
  await msalInstance.initialize()

  // Without this, MSAL leaves the active account unset after a redirect login completes —
  // every acquireTokenSilent call downstream (client.ts) would otherwise have no account to
  // silently reauthenticate, forcing an interactive prompt on literally the first API call.
  const existing = msalInstance.getActiveAccount()
  if (!existing) {
    const accounts = msalInstance.getAllAccounts()
    if (accounts.length > 0) msalInstance.setActiveAccount(accounts[0])
  }

  msalInstance.addEventCallback((event) => {
    if (event.eventType === EventType.LOGIN_SUCCESS && event.payload) {
      const account = (event.payload as { account: AccountInfo }).account
      msalInstance.setActiveAccount(account)
    }
  })
})()
