import { apiRequest } from './client'
import type { UserRole } from './users'

export interface ScreenPermission {
  role: UserRole
  screenKey: string
  isVisible: boolean
}

// Keep in sync with AuditIQ.Domain.Common.ScreenKeys — adding a screen means adding a key here
// too, plus a matching seed row per role on the backend.
export const ScreenKeys = {
  Dashboard: 'dashboard',
  Score: 'score',
  Review: 'review',
  Calibration: 'calibration',
  Reports: 'reports',
  Scorecards: 'scorecards',
  Staff: 'staff',
} as const

export const screenPermissionsApi = {
  list: () => apiRequest<ScreenPermission[]>('/api/v1/screen-permissions'),

  update: (permissions: ScreenPermission[]) =>
    apiRequest<void>('/api/v1/screen-permissions', { method: 'PUT', body: JSON.stringify({ permissions }) }),
}
