import { useEffect, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Save } from 'lucide-react'
import { SectionHeader } from '@/components/ui/SectionHeader'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { screenPermissionsApi, ScreenKeys, type ScreenPermission } from '@/api/screenPermissions'
import type { UserRole } from '@/api/users'

const roles: UserRole[] = [
  'Admin', 'Supervisor', 'TeamLead', 'QaEvaluator', 'Agent',
  'GroupAdmin', 'TeamAdmin', 'ReportsAnalyst', 'CalibrationAnalyst',
]

const screens: { key: string; label: string }[] = [
  { key: ScreenKeys.Dashboard, label: 'Dashboard' },
  { key: ScreenKeys.Score, label: 'Score' },
  { key: ScreenKeys.Review, label: 'Review' },
  { key: ScreenKeys.Calibration, label: 'Calibration' },
  { key: ScreenKeys.Reports, label: 'Reports' },
  { key: ScreenKeys.Scorecards, label: 'Scorecards' },
  { key: ScreenKeys.Staff, label: 'Staff' },
]

// Admin's own row is fixed on (can't be unchecked in the UI) — an Admin locking themselves out
// of a screen here is almost certainly a mistake, and there's no other path back into this page
// if Settings itself ever became conditional on it.
const ADMIN_ROLE: UserRole = 'Admin'

export function PermissionsPage() {
  const queryClient = useQueryClient()
  const permissionsQuery = useQuery({ queryKey: ['screen-permissions'], queryFn: () => screenPermissionsApi.list() })
  const [grid, setGrid] = useState<Record<string, boolean> | null>(null)

  useEffect(() => {
    if (!permissionsQuery.data) return
    const next: Record<string, boolean> = {}
    for (const screen of screens) {
      for (const role of roles) {
        const key = `${role}:${screen.key}`
        const match = permissionsQuery.data.find((p) => p.role === role && p.screenKey === screen.key)
        next[key] = role === ADMIN_ROLE ? true : (match?.isVisible ?? false)
      }
    }
    setGrid(next)
  }, [permissionsQuery.data])

  const updateMutation = useMutation({
    mutationFn: (permissions: ScreenPermission[]) => screenPermissionsApi.update(permissions),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['screen-permissions'] }),
  })

  function toggle(role: UserRole, screenKey: string) {
    if (role === ADMIN_ROLE) return
    setGrid((prev) => (prev ? { ...prev, [`${role}:${screenKey}`]: !prev[`${role}:${screenKey}`] } : prev))
  }

  function handleSave() {
    if (!grid) return
    const permissions: ScreenPermission[] = []
    for (const screen of screens) {
      for (const role of roles) {
        permissions.push({ role, screenKey: screen.key, isVisible: grid[`${role}:${screen.key}`] })
      }
    }
    updateMutation.mutate(permissions)
  }

  return (
    <>
      <SectionHeader
        title="Screen permissions"
        description="Control which roles can see which parts of the workspace. This only hides navigation and routes — it does not change what the API allows a role to do."
        action={
          <Button onClick={handleSave} disabled={!grid || updateMutation.isPending}>
            <Save className="size-4" />
            {updateMutation.isPending ? 'Saving...' : 'Save changes'}
          </Button>
        }
      />

      {permissionsQuery.isLoading && <p className="text-sm text-muted-foreground">Loading...</p>}
      {permissionsQuery.isError && (
        <p className="text-sm text-status-fail">Failed to load: {(permissionsQuery.error as Error).message}</p>
      )}
      {updateMutation.isError && (
        <p className="mb-4 text-sm text-status-fail">{(updateMutation.error as Error).message}</p>
      )}
      {updateMutation.isSuccess && <p className="mb-4 text-sm text-status-pass">Saved.</p>}

      {grid && (
        <Card className="overflow-x-auto">
          <table className="w-full min-w-[600px] text-sm">
            <thead>
              <tr className="border-b border-border text-left text-muted-foreground">
                <th className="py-2 pr-4 font-medium">Screen</th>
                {roles.map((role) => (
                  <th key={role} className="px-3 py-2 text-center font-medium">{role}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {screens.map((screen) => (
                <tr key={screen.key} className="border-b border-border last:border-0">
                  <td className="py-3 pr-4 font-medium">{screen.label}</td>
                  {roles.map((role) => (
                    <td key={role} className="px-3 py-3 text-center">
                      <input
                        type="checkbox"
                        checked={grid[`${role}:${screen.key}`]}
                        disabled={role === ADMIN_ROLE}
                        onChange={() => toggle(role, screen.key)}
                        className="size-4 disabled:opacity-50"
                        title={role === ADMIN_ROLE ? 'Admin always has full access' : undefined}
                      />
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </Card>
      )}
    </>
  )
}
