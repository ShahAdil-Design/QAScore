import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Plus, Search } from 'lucide-react'
import { SectionHeader } from '@/components/ui/SectionHeader'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { DataTable } from '@/components/ui/DataTable'
import { usersApi, type CreateUserInput } from '@/api/users'
import { teamsApi } from '@/api/teams'
import { groupsApi } from '@/api/groups'
import { CreateUserForm } from '@/features/staff/CreateUserForm'
import { useCurrentUser } from '@/auth/currentUserStore'

export function StaffPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [isCreating, setIsCreating] = useState(false)
  const [search, setSearch] = useState('')
  const me = useCurrentUser((s) => s.me)
  const isAdmin = !me || me.role === 'Admin'

  const usersQuery = useQuery({ queryKey: ['users', 'all'], queryFn: () => usersApi.list(undefined, true) })
  const teamsQuery = useQuery({ queryKey: ['teams'], queryFn: () => teamsApi.list(), enabled: isCreating })
  const groupsQuery = useQuery({ queryKey: ['groups'], queryFn: () => groupsApi.list(), enabled: isCreating })

  const createMutation = useMutation({
    mutationFn: (input: CreateUserInput) => usersApi.create(input),
    onSuccess: (result) => {
      queryClient.invalidateQueries({ queryKey: ['users', 'all'] })
      setIsCreating(false)
      navigate(`/staff/${result.id}`)
    },
  })

  const users = usersQuery.data ?? []
  const filteredUsers = users.filter((u) => {
    const q = search.trim().toLowerCase()
    if (!q) return true
    return u.displayName.toLowerCase().includes(q) || u.email.toLowerCase().includes(q)
  })

  return (
    <>
      <SectionHeader
        title="Staff"
        description="Roles & responsibilities — user profiles, role assignment, and team/group membership."
        action={
          isAdmin && !isCreating && (
            <Button onClick={() => setIsCreating(true)}>
              <Plus className="size-4" /> New user
            </Button>
          )
        }
      />

      {isCreating && (
        <div className="mb-6">
          <CreateUserForm
            teams={teamsQuery.data ?? []}
            groups={groupsQuery.data ?? []}
            isSubmitting={createMutation.isPending}
            onCancel={() => setIsCreating(false)}
            onSubmit={(input) => createMutation.mutate(input)}
          />
          {createMutation.isError && (
            <p className="mt-2 text-sm text-status-fail">{(createMutation.error as Error).message}</p>
          )}
        </div>
      )}

      {usersQuery.isLoading && <p className="text-sm text-muted-foreground">Loading staff...</p>}
      {usersQuery.isError && (
        <p className="text-sm text-status-fail">Failed to load staff: {(usersQuery.error as Error).message}</p>
      )}

      {usersQuery.isSuccess && users.length > 0 && (
        <div className="relative mb-4 max-w-sm">
          <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search staff by name or email..."
            className="w-full rounded-lg border border-input bg-background py-2 pl-9 pr-3 text-sm outline-none focus:ring-2 focus:ring-ring"
          />
        </div>
      )}

      {usersQuery.isSuccess && users.length > 0 && filteredUsers.length === 0 && (
        <Card className="text-sm text-muted-foreground">No staff match "{search}".</Card>
      )}

      {!usersQuery.isLoading && filteredUsers.length > 0 && (
        <div className="overflow-hidden rounded-xl border border-border bg-card shadow-sm">
          <DataTable
            headers={['Name', 'Email', 'Role', 'Status']}
            rows={filteredUsers.map((u) => [
              <button
                key="name"
                onClick={() => navigate(`/staff/${u.id}`)}
                className="font-medium text-foreground hover:text-primary hover:underline"
              >
                {u.displayName}
              </button>,
              u.email,
              <Badge tone="primary" key="role">{u.role}</Badge>,
              <Badge tone={u.isActive ? 'pass' : 'neutral'} key="status">{u.isActive ? 'Active' : 'Deactivated'}</Badge>,
            ])}
          />
        </div>
      )}
    </>
  )
}
