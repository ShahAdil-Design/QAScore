import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft } from 'lucide-react'
import { Card } from '@/components/ui/Card'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { usersApi, type UserRole } from '@/api/users'
import { teamsApi } from '@/api/teams'
import { groupsApi } from '@/api/groups'
import { useBreadcrumb } from '@/lib/breadcrumb'
import { useCurrentUser } from '@/auth/currentUserStore'

const roles: UserRole[] = [
  'Admin', 'Supervisor', 'TeamLead', 'QaEvaluator', 'Agent',
  'GroupAdmin', 'TeamAdmin', 'ReportsAnalyst', 'CalibrationAnalyst',
]

export function EditStaffPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const setExtra = useBreadcrumb((s) => s.setExtra)
  const me = useCurrentUser((s) => s.me)
  const isAdmin = !me || me.role === 'Admin'
  const readOnly = !isAdmin

  const userQuery = useQuery({ queryKey: ['user', id], queryFn: () => usersApi.getById(id!), enabled: !!id })
  const teamsQuery = useQuery({ queryKey: ['teams'], queryFn: () => teamsApi.list() })
  const groupsQuery = useQuery({ queryKey: ['groups'], queryFn: () => groupsApi.list() })

  const [displayName, setDisplayName] = useState('')
  const [email, setEmail] = useState('')
  const [role, setRole] = useState<UserRole>('Agent')
  const [employmentType, setEmploymentType] = useState('')
  const [notes, setNotes] = useState('')
  const [teamIds, setTeamIds] = useState<string[]>([])
  const [groupIds, setGroupIds] = useState<string[]>([])
  const [loaded, setLoaded] = useState(false)

  useEffect(() => {
    if (userQuery.data && !loaded) {
      setDisplayName(userQuery.data.displayName)
      setEmail(userQuery.data.email)
      setRole(userQuery.data.role)
      setEmploymentType(userQuery.data.employmentType ?? '')
      setNotes(userQuery.data.notes ?? '')
      setTeamIds(userQuery.data.teams.map((t) => t.teamId))
      setGroupIds(userQuery.data.groups.map((g) => g.groupId))
      setLoaded(true)
    }
  }, [userQuery.data, loaded])

  useEffect(() => {
    setExtra(userQuery.data?.displayName ?? null)
    return () => setExtra(null)
  }, [userQuery.data?.displayName, setExtra])

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['user', id] })
    queryClient.invalidateQueries({ queryKey: ['users', 'all'] })
  }

  const updateMutation = useMutation({
    mutationFn: () => usersApi.update(id!, { displayName, email, role, employmentType: employmentType || null, notes: notes || null }),
    onSuccess: invalidate,
  })

  const setTeamsMutation = useMutation({
    mutationFn: (ids: string[]) => usersApi.setTeams(id!, ids),
    onSuccess: invalidate,
  })

  const setGroupsMutation = useMutation({
    mutationFn: (ids: string[]) => usersApi.setGroups(id!, ids),
    onSuccess: invalidate,
  })

  const deactivateMutation = useMutation({ mutationFn: () => usersApi.deactivate(id!), onSuccess: invalidate })
  const reactivateMutation = useMutation({ mutationFn: () => usersApi.reactivate(id!), onSuccess: invalidate })

  function toggleTeam(teamId: string) {
    if (readOnly) return
    const next = teamIds.includes(teamId) ? teamIds.filter((x) => x !== teamId) : [...teamIds, teamId]
    setTeamIds(next)
    setTeamsMutation.mutate(next)
  }

  function toggleGroup(groupId: string) {
    if (readOnly) return
    const next = groupIds.includes(groupId) ? groupIds.filter((x) => x !== groupId) : [...groupIds, groupId]
    setGroupIds(next)
    setGroupsMutation.mutate(next)
  }

  const inputClass = 'w-full rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring'

  return (
    <>
      <button
        onClick={() => navigate('/staff')}
        className="mb-4 flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
      >
        <ArrowLeft className="size-4" /> Back to Staff
      </button>

      {userQuery.isLoading && <Card className="text-sm text-muted-foreground">Loading user...</Card>}
      {userQuery.isError && (
        <Card className="text-sm text-status-fail">Failed to load: {(userQuery.error as Error).message}</Card>
      )}

      {userQuery.data && (
        <>
          <div className="mb-6 flex items-center gap-3">
            <h2 className="text-2xl font-semibold tracking-tight">{userQuery.data.displayName}</h2>
            <Badge tone={userQuery.data.isActive ? 'pass' : 'neutral'}>
              {userQuery.data.isActive ? 'Active' : 'Deactivated'}
            </Badge>
          </div>

          <Card>
            <div className="grid gap-4 md:grid-cols-2">
              <label className="flex flex-col gap-1.5 text-sm">
                <span className="font-medium">Name</span>
                <input disabled={readOnly} className={inputClass} value={displayName} onChange={(e) => setDisplayName(e.target.value)} />
              </label>
              <label className="flex flex-col gap-1.5 text-sm">
                <span className="font-medium">Email</span>
                <input disabled={readOnly} type="email" className={inputClass} value={email} onChange={(e) => setEmail(e.target.value)} />
              </label>
              <label className="flex flex-col gap-1.5 text-sm">
                <span className="font-medium">Role</span>
                <select disabled={readOnly} className={inputClass} value={role} onChange={(e) => setRole(e.target.value as UserRole)}>
                  {roles.map((r) => (
                    <option key={r} value={r}>{r}</option>
                  ))}
                </select>
              </label>
              <label className="flex flex-col gap-1.5 text-sm">
                <span className="font-medium">Employment type</span>
                <input disabled={readOnly} className={inputClass} value={employmentType} onChange={(e) => setEmploymentType(e.target.value)} />
              </label>
              <label className="flex flex-col gap-1.5 text-sm md:col-span-2">
                <span className="font-medium">Notes</span>
                <input disabled={readOnly} className={inputClass} value={notes} onChange={(e) => setNotes(e.target.value)} />
              </label>
            </div>

            {!readOnly && (
              <div className="mt-4 flex justify-end gap-2">
                <Button variant="secondary" onClick={() => (userQuery.data!.isActive ? deactivateMutation.mutate() : reactivateMutation.mutate())}>
                  {userQuery.data.isActive ? 'Deactivate' : 'Reactivate'}
                </Button>
                <Button onClick={() => updateMutation.mutate()} disabled={updateMutation.isPending}>
                  {updateMutation.isPending ? 'Saving...' : 'Save profile & role'}
                </Button>
              </div>
            )}
            {updateMutation.isError && <p className="mt-2 text-sm text-status-fail">{(updateMutation.error as Error).message}</p>}

            <div className="mt-6 flex flex-col gap-2 text-sm">
              <span className="font-medium">Groups</span>
              <div className="flex flex-wrap gap-2">
                {groupsQuery.data?.map((g) => (
                  <button
                    key={g.id}
                    onClick={() => toggleGroup(g.id)}
                    className={
                      'rounded-lg border px-3 py-1.5 text-xs font-medium transition-colors ' +
                      (groupIds.includes(g.id)
                        ? 'border-primary bg-primary/10 text-primary'
                        : 'border-border text-muted-foreground hover:border-primary/50 hover:text-foreground')
                    }
                  >
                    {g.name}
                  </button>
                ))}
              </div>
            </div>

            <div className="mt-4 flex flex-col gap-2 text-sm">
              <span className="font-medium">Teams</span>
              <div className="flex max-h-40 flex-wrap gap-2 overflow-y-auto">
                {teamsQuery.data?.map((t) => (
                  <button
                    key={t.id}
                    onClick={() => toggleTeam(t.id)}
                    className={
                      'rounded-lg border px-3 py-1.5 text-xs font-medium transition-colors ' +
                      (teamIds.includes(t.id)
                        ? 'border-primary bg-primary/10 text-primary'
                        : 'border-border text-muted-foreground hover:border-primary/50 hover:text-foreground')
                    }
                  >
                    {t.name}
                  </button>
                ))}
              </div>
            </div>
          </Card>
        </>
      )}
    </>
  )
}
