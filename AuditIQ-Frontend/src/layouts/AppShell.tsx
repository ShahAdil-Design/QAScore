import { useState } from 'react'
import { Outlet } from 'react-router-dom'
import { Sidebar } from '@/components/Sidebar'
import { Header } from '@/components/Header'
import { ScrollToTop } from '@/components/ScrollToTop'
import { RequireAuth } from '@/auth/RequireAuth'
import { RequireScreen } from '@/auth/RequireScreen'

export function AppShell() {
  const [sidebarOpen, setSidebarOpen] = useState(false)

  return (
    <RequireAuth>
      <ScrollToTop />
      <div className="min-h-screen bg-background text-foreground">
        <Sidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} />

        <div className="lg:pl-64">
          <Header onOpenSidebar={() => setSidebarOpen(true)} />
          <main className="mx-auto max-w-[var(--content-max)] p-5 sm:p-8">
            <RequireScreen>
              <Outlet />
            </RequireScreen>
          </main>
        </div>
      </div>
    </RequireAuth>
  )
}
