import { useLocation } from 'react-router-dom'
import { Menu, Moon, Sun } from 'lucide-react'
import { useTheme } from '@/lib/theme'
import { useBreadcrumb } from '@/lib/breadcrumb'
import { useCurrentUser } from '@/auth/currentUserStore'
import { navItems } from './Sidebar'
import { NotificationsBell } from './NotificationsBell'

export function Header({ onOpenSidebar }: { onOpenSidebar: () => void }) {
  const location = useLocation()
  const theme = useTheme((s) => s.theme)
  const toggle = useTheme((s) => s.toggle)
  const extra = useBreadcrumb((s) => s.extra)
  const me = useCurrentUser((s) => s.me)

  const current = navItems.find((item) =>
    item.end ? location.pathname === item.to : location.pathname === item.to || location.pathname.startsWith(`${item.to}/`),
  )
  const label = current?.label ?? 'AuditIQ'

  return (
    <header className="flex h-20 items-center justify-between border-b border-border bg-card px-5 sm:px-8">
      <div className="flex items-center gap-3">
        <button
          aria-label="Open navigation"
          className="rounded-lg p-2 hover:bg-muted lg:hidden"
          onClick={onOpenSidebar}
        >
          <Menu className="size-5" />
        </button>
        <div className="hidden items-center gap-2 text-sm text-muted-foreground sm:flex">
          <span>Workspace</span>
          <span>/</span>
          {extra ? (
            <>
              <span>{label}</span>
              <span>/</span>
              <span className="font-medium text-foreground">{extra}</span>
            </>
          ) : (
            <span className="font-medium text-foreground">{label}</span>
          )}
        </div>
        <h1 className="text-lg font-semibold sm:hidden">{extra ?? label}</h1>
      </div>

      <div className="flex items-center gap-3">
        {me && (
          <div className="hidden items-center gap-2 rounded-full border border-border bg-muted/50 px-3 py-1 text-xs sm:flex">
            <span className="font-medium text-foreground">{me.displayName}</span>
            <span className="text-muted-foreground">·</span>
            <span className="text-primary">{me.role}</span>
          </div>
        )}
        <NotificationsBell />
        <button
          onClick={toggle}
          aria-label="Toggle theme"
          title="Toggle theme"
          className="rounded-lg p-2 text-muted-foreground hover:bg-muted"
        >
          {theme === 'dark' ? <Sun className="size-[18px]" /> : <Moon className="size-[18px]" />}
        </button>
      </div>
    </header>
  )
}
