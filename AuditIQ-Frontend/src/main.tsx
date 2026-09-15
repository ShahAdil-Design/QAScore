import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MsalProvider } from '@azure/msal-react'
import './index.css'
import App from './App.tsx'
import { msalInstance, msalInitialized } from './auth/msalInstance'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      staleTime: 30_000,
    },
  },
})

// MsalProvider is always mounted, even with placeholder config (isSsoConfigured false) — hooks
// like useIsAuthenticated() require the provider to exist in the tree regardless of whether
// it's actually wired to a real tenant yet; constructing PublicClientApplication with
// placeholder values makes no network call until a login method is actually invoked, so this
// is inert until real values replace the placeholders (see msalConfig.ts / .env.example).
async function main() {
  await msalInitialized

  createRoot(document.getElementById('root')!).render(
    <StrictMode>
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <MsalProvider instance={msalInstance}>
            <App />
          </MsalProvider>
        </BrowserRouter>
      </QueryClientProvider>
    </StrictMode>,
  )
}

main()
