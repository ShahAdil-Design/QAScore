import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ClipboardList, Plus, Search } from 'lucide-react'
import { SectionHeader } from '@/components/ui/SectionHeader'
import { Button } from '@/components/ui/Button'
import { DataTable } from '@/components/ui/DataTable'
import { calibrationApi } from '@/api/calibration'
import { useCurrentUser } from '@/auth/currentUserStore'
import { NewCalibrationListModal } from './NewCalibrationListModal'

export function CalibrationPage() {
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [isCreating, setIsCreating] = useState(false)
  const queryClient = useQueryClient()
  const me = useCurrentUser((s) => s.me)

  const listsQuery = useQuery({
    queryKey: ['calibration-lists', search],
    queryFn: () => calibrationApi.getLists(search || undefined),
  })

  const createMutation = useMutation({
    mutationFn: (input: { name: string; visibilityScope: string }) =>
      calibrationApi.create({ ...input, createdByUserId: me?.id ?? '' }),
    onSuccess: (result) => {
      queryClient.invalidateQueries({ queryKey: ['calibration-lists'] })
      setIsCreating(false)
      navigate(`/calibration/${result.id}/builder`)
    },
  })

  const lists = listsQuery.data?.items ?? []

  return (
    <>
      <SectionHeader
        title="Calibration Lists"
        description="Batches of evaluations pulled together for calibration — filter/sample or manually pick evaluations into a list, then anyone eligible can calibrate its items."
        action={
          <Button onClick={() => setIsCreating(true)}>
            <Plus className="size-4" /> Add List
          </Button>
        }
      />

      <div className="relative mb-4 max-w-sm">
        <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search lists..."
          className="w-full rounded-lg border border-input bg-background py-2 pl-9 pr-3 text-sm outline-none focus:ring-2 focus:ring-ring"
        />
      </div>

      {listsQuery.isLoading && <p className="text-sm text-muted-foreground">Loading calibration lists...</p>}
      {listsQuery.isError && (
        <p className="text-sm text-status-fail">Failed to load: {(listsQuery.error as Error).message}</p>
      )}

      {!listsQuery.isLoading && (
        <div className="overflow-hidden rounded-xl border border-border bg-card shadow-sm">
          <DataTable
            headers={['List name', 'User types', 'Created by', 'Date created', 'Group', 'Team', 'Items', 'Actions']}
            rows={lists.map((l) => [
              <span className="font-medium text-foreground" key="name">{l.name}</span>,
              l.visibilityScope,
              l.createdByName,
              new Date(l.createdAt).toLocaleDateString(),
              l.groupNames.join(', ') || '—',
              l.teamNames.join(', ') || '—',
              `${l.ratedItemCount}/${l.itemCount} calibrated`,
              <div className="flex gap-2" key="actions">
                <Button className="h-7 px-2 text-xs" onClick={() => navigate(`/calibration/${l.id}/items`)}>
                  <ClipboardList className="size-3" /> Calibrate
                </Button>
                <Button variant="secondary" className="h-7 px-2 text-xs" onClick={() => navigate(`/calibration/${l.id}/builder`)}>
                  Add evaluations
                </Button>
              </div>,
            ])}
          />
        </div>
      )}

      {isCreating && (
        <NewCalibrationListModal
          isSubmitting={createMutation.isPending}
          error={createMutation.isError ? (createMutation.error as Error).message : null}
          onCancel={() => setIsCreating(false)}
          onSubmit={(input) => createMutation.mutate(input)}
        />
      )}
    </>
  )
}
