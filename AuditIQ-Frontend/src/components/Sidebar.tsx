import { NavLink, useNavigate } from 'react-router-dom'
import clsx from 'clsx'
import {
  Activity,
  BarChart3,
  BookOpenCheck,
  ClipboardCheck,
  LayoutDashboard,
  Settings2,
  UserCheck,
  Users,
  X,
} from 'lucide-react'
import OakbrookCircle from '@/icons/OakbrookCircle'
import { useCurrentUser } from '@/auth/currentUserStore'
import { useScreenAccess } from '@/auth/useScreenAccess'
import { ScreenKeys } from '@/api/screenPermissions'

interface NavItem {
  to: string
  label: string
  icon: typeof LayoutDashboard
  phase2?: boolean
  end?: boolean
  screenKey: string
}

const navItems: NavItem[] = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard, end: true, screenKey: ScreenKeys.Dashboard },
  { to: '/score', label: 'Score', icon: ClipboardCheck, screenKey: ScreenKeys.Score },
  { to: '/review', label: 'Review', icon: UserCheck, screenKey: ScreenKeys.Review },
  { to: '/calibration', label: 'Calibration', icon: Activity, screenKey: ScreenKeys.Calibration },
  { to: '/reports', label: 'Reports', icon: BarChart3, screenKey: ScreenKeys.Reports },
  { to: '/scorecards', label: 'Scorecards', icon: BookOpenCheck, screenKey: ScreenKeys.Scorecards },
  { to: '/staff', label: 'Staff', icon: Users, screenKey: ScreenKeys.Staff },
]

export function Sidebar({ open, onClose }: { open: boolean; onClose: () => void }) {
  const me = useCurrentUser((s) => s.me)
  const navigate = useNavigate()
  const { canSeeScreen } = useScreenAccess()
  const isAdmin = !me || me.role === 'Admin'
  const visibleItems = navItems.filter((item) => canSeeScreen(item.screenKey))

  return (
    <>
      <aside
        className={clsx(
          'fixed inset-y-0 left-0 z-30 flex w-64 flex-col border-r border-border bg-card transition-transform lg:translate-x-0',
          open ? 'translate-x-0' : '-translate-x-full',
        )}
      >
        <div className="flex h-20 items-center gap-3 border-b border-border px-6">
          <div className="flex size-9 items-center justify-center">
            <OakbrookCircle className="size-9" />
          </div>
          <div>
            <div className="font-semibold tracking-tight">AuditIQ</div>
            <div className="text-xs text-muted-foreground">Quality evaluation</div>
          </div>
          <button
            aria-label="Close navigation"
            className="ml-auto rounded-lg p-1 text-muted-foreground hover:bg-muted lg:hidden"
            onClick={onClose}
          >
            <X className="size-4" />
          </button>
        </div>

        <div className="flex flex-1 flex-col px-3 py-5">
          <div className="px-3 pb-3 text-[11px] font-semibold uppercase tracking-widest text-muted-foreground">
            Workspace
          </div>
          <nav className="flex flex-col gap-1" aria-label="Main navigation">
            {visibleItems.map(({ to, label, icon: Icon, phase2, end }) => (
              <NavLink
                key={to}
                to={to}
                end={end}
                onClick={onClose}
                className={({ isActive }) =>
                  clsx(
                    'flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm hover:no-underline',
                    isActive
                      ? 'bg-primary/10 font-medium text-primary'
                      : 'text-muted-foreground hover:bg-muted hover:text-foreground',
                  )
                }
              >
                <Icon className="size-[18px]" />
                <span className="flex-1 text-left">{label}</span>
                {phase2 && (
                  <span className="rounded-full bg-muted px-2 py-0.5 text-[10px] uppercase tracking-wide text-muted-foreground">
                    Phase 2
                  </span>
                )}
              </NavLink>
            ))}
          </nav>

          {isAdmin && (
            <div className="mt-auto flex flex-col gap-1">
              <button
                onClick={() => { navigate('/settings/permissions'); onClose() }}
                className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-muted-foreground hover:bg-muted"
              >
                <Settings2 className="size-[18px]" />
                Settings
              </button>
            </div>
          )}
        </div>
      </aside>

      {open && (
        <button
          aria-label="Close navigation overlay"
          className="fixed inset-0 z-20 bg-foreground/20 lg:hidden"
          onClick={onClose}
        />
      )}
    </>
  )
}

export { navItems }
