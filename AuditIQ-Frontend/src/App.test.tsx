import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { BrowserRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import App from './App'

function renderApp() {
  const queryClient = new QueryClient()
  return render(
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <App />
      </BrowserRouter>
    </QueryClientProvider>,
  )
}

describe('App', () => {
  it('renders the Dashboard placeholder for unauthenticated users', () => {
    renderApp()
    expect(screen.getByText('Sign in to AuditIQ')).toBeInTheDocument()
  })
})
