import { useNavigate, useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ArrowLeft, Eye, Plus } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { DataTable } from '@/components/ui/DataTable'
import { calibrationApi } from '@/api/calibration'
import { useCurrentUser } from '@/auth/currentUserStore'

export function CalibrationItemsPage() {
  const { listId } = useParams<{ listId: string }>()
  const navigate = useNavigate()
  const me = useCurrentUser((s) => s.me)

  const itemsQuery = useQuery({
    queryKey: ['calibration-items', listId, me?.id],
    queryFn: () => calibrationApi.getItems(listId!, me?.id),
    enabled: !!listId,
  })

  const items = itemsQuery.data?.items ?? []

  return (
    <>
      <button
        onClick={() => navigate('/calibration')}
        className="mb-4 flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
      >
        <ArrowLeft className="size-4" /> Back to Calibration Lists
      </button>

      <div className="mb-6 flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-semibold tracking-tight">Calibrate</h2>
          <p className="mt-1 text-sm text-muted-foreground">Pick an item below to re-score it against the original evaluator.</p>
        </div>
        <Button variant="secondary" onClick={() => navigate(`/calibration/${listId}/builder`)}>
          <Plus className="size-4" /> Add evaluations
        </Button>
      </div>

      {itemsQuery.isLoading && <p className="text-sm text-muted-foreground">Loading items...</p>}
      {itemsQuery.isError && (
        <p className="text-sm text-status-fail">Failed to load: {(itemsQuery.error as Error).message}</p>
      )}

      {!itemsQuery.isLoading && items.length === 0 && (
        <p className="text-sm text-muted-foreground">No evaluations in this list yet — add some to get started.</p>
      )}

      {!itemsQuery.isLoading && items.length > 0 && (
        <div className="overflow-hidden rounded-xl border border-border bg-card shadow-sm">
          <DataTable
            headers={['Employee', 'Evaluator', 'Event date', 'Team', 'Scorecard', 'Reference', 'Original score', 'My score', 'Raters', 'Actions']}
            rows={items.map((i) => [
              i.agentName,
              i.evaluatorName,
              i.eventOccurredAt ? new Date(i.eventOccurredAt).toLocaleDateString() : '—',
              i.teamNames.join(', ') || '—',
              i.scorecardName,
              i.reference ?? '—',
              i.originalScore !== null ? `${i.originalScore}%` : '—',
              i.myScore !== null ? `${i.myScore}%` : '—',
              i.ratingCount,
              <Button
                key="calibrate"
                variant="secondary"
                className="h-7 px-2 text-xs"
                onClick={() => navigate(`/calibration/items/${i.id}`)}
              >
                <Eye className="size-3" /> {i.myScore !== null ? 'View / edit' : 'Calibrate'}
              </Button>,
            ])}
          />
        </div>
      )}
    </>
  )
}
