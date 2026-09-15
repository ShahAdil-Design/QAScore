import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { SectionHeader } from '@/components/ui/SectionHeader'
import { Badge } from '@/components/ui/Badge'
import { Card } from '@/components/ui/Card'
import { DataTable } from '@/components/ui/DataTable'
import { auditApi, type AuditAction } from '@/api/audit'

const ENTITY_OPTIONS = ['User', 'Scorecard', 'Evaluation']
const ACTION_OPTIONS: AuditAction[] = ['Created', 'Updated', 'Deleted']
const PAGE_SIZE = 25

const actionTone: Record<AuditAction, 'pass' | 'primary' | 'fail'> = {
  Created: 'pass',
  Updated: 'primary',
  Deleted: 'fail',
}

function formatChanges(changes: string | null): string {
  if (!changes) return '—'
  try {
    return JSON.stringify(JSON.parse(changes), null, 2)
  } catch {
    return changes
  }
}

export function AuditLogPage() {
  const [entityName, setEntityName] = useState('')
  const [action, setAction] = useState<AuditAction | ''>('')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')
  const [page, setPage] = useState(1)

  const filters = {
    entityName: entityName || undefined,
    action: action || undefined,
    dateFrom: dateFrom ? new Date(dateFrom).toISOString() : undefined,
    dateTo: dateTo ? new Date(dateTo).toISOString() : undefined,
    page,
    pageSize: PAGE_SIZE,
  }

  const auditQuery = useQuery({
    queryKey: ['audit-log', filters],
    queryFn: () => auditApi.getLog(filters),
  })

  const resetPage = () => setPage(1)

  return (
    <>
      <SectionHeader
        title="Audit Log"
        description="Who created, edited, or deleted a user or scorecard, and when."
      />

      <Card className="mb-4">
        <div className="flex flex-wrap gap-3">
          <select
            value={entityName}
            onChange={(e) => { setEntityName(e.target.value); resetPage() }}
            className="rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring"
          >
            <option value="">All entity types</option>
            {ENTITY_OPTIONS.map((o) => <option key={o} value={o}>{o}</option>)}
          </select>

          <select
            value={action}
            onChange={(e) => { setAction(e.target.value as AuditAction | ''); resetPage() }}
            className="rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring"
          >
            <option value="">All actions</option>
            {ACTION_OPTIONS.map((o) => <option key={o} value={o}>{o}</option>)}
          </select>

          <input
            type="date"
            value={dateFrom}
            onChange={(e) => { setDateFrom(e.target.value); resetPage() }}
            className="rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring"
          />
          <span className="self-center text-sm text-muted-foreground">to</span>
          <input
            type="date"
            value={dateTo}
            onChange={(e) => { setDateTo(e.target.value); resetPage() }}
            className="rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring"
          />
        </div>
      </Card>

      {auditQuery.isLoading && <p className="text-sm text-muted-foreground">Loading audit log...</p>}
      {auditQuery.isError && (
        <p className="text-sm text-status-fail">Failed to load audit log: {(auditQuery.error as Error).message}</p>
      )}

      {auditQuery.isSuccess && (
        <>
          <div className="overflow-hidden rounded-xl border border-border bg-card shadow-sm">
            <DataTable
              headers={['Timestamp', 'Actor', 'Action', 'Entity', 'Changes']}
              emptyLabel="No audit activity matches these filters."
              rows={auditQuery.data.items.map((entry) => [
                new Date(entry.timestampUtc).toLocaleString(),
                entry.userDisplayName ?? <span className="italic text-muted-foreground">System</span>,
                <Badge key="action" tone={actionTone[entry.action]}>{entry.action}</Badge>,
                <span key="entity">{entry.entityName} <span className="text-xs text-muted-foreground">({entry.entityId})</span></span>,
                <pre key="changes" className="max-w-md whitespace-pre-wrap break-words font-mono text-xs">
                  {formatChanges(entry.changes)}
                </pre>,
              ])}
            />
          </div>

          <div className="mt-4 flex items-center justify-between text-sm text-muted-foreground">
            <span>
              {auditQuery.data.totalCount === 0
                ? 'No results'
                : `Page ${auditQuery.data.page} of ${auditQuery.data.totalPages} (${auditQuery.data.totalCount} total)`}
            </span>
            <div className="flex gap-2">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page <= 1}
                className="flex items-center gap-1 rounded-lg border border-input px-3 py-1.5 disabled:opacity-40"
              >
                <ChevronLeft className="size-4" /> Prev
              </button>
              <button
                onClick={() => setPage((p) => p + 1)}
                disabled={page >= auditQuery.data.totalPages}
                className="flex items-center gap-1 rounded-lg border border-input px-3 py-1.5 disabled:opacity-40"
              >
                Next <ChevronRight className="size-4" />
              </button>
            </div>
          </div>
        </>
      )}
    </>
  )
}
