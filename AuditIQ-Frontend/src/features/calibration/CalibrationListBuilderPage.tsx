import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, Filter, Shuffle } from 'lucide-react'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { DataTable } from '@/components/ui/DataTable'
import { calibrationApi, type CalibrationCandidateFilters } from '@/api/calibration'
import { groupsApi } from '@/api/groups'
import { teamsApi } from '@/api/teams'
import { scorecardsApi } from '@/api/scorecards'
import { scorecardCategoriesApi } from '@/api/scorecardCategories'
import { usersApi } from '@/api/users'

const inputClass = 'rounded-lg border border-input bg-background px-2.5 py-1.5 text-sm outline-none focus:ring-2 focus:ring-ring'

export function CalibrationListBuilderPage() {
  const { listId } = useParams<{ listId: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const [showFilters, setShowFilters] = useState(true)
  const [filters, setFilters] = useState<CalibrationCandidateFilters>({ page: 1, pageSize: 25 })
  const [sampleSize, setSampleSize] = useState(50)
  const [selected, setSelected] = useState<Set<string>>(new Set())

  const groupsQuery = useQuery({ queryKey: ['groups'], queryFn: () => groupsApi.list() })
  const teamsQuery = useQuery({ queryKey: ['teams', filters.groupId], queryFn: () => teamsApi.list(filters.groupId) })
  const scorecardsQuery = useQuery({ queryKey: ['scorecards'], queryFn: () => scorecardsApi.list() })
  const categoriesQuery = useQuery({ queryKey: ['scorecard-categories'], queryFn: () => scorecardCategoriesApi.list() })
  const evaluatorsQuery = useQuery({
    queryKey: ['users', 'evaluator-eligible'],
    queryFn: () => usersApi.list(),
    select: (users) => users.filter((u) => u.role !== 'Agent'),
  })

  const candidatesQuery = useQuery({
    queryKey: ['calibration-candidates', filters],
    queryFn: () => calibrationApi.getCandidates(filters),
  })

  const addItemsMutation = useMutation({
    mutationFn: (evaluationIds: string[]) => calibrationApi.addItems(listId!, evaluationIds),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['calibration-lists'] })
      navigate(`/calibration/${listId}/items`)
    },
  })

  const randomSampleMutation = useMutation({
    mutationFn: () => calibrationApi.getCandidates({ ...filters, random: true, sampleSize }),
    onSuccess: (result) => addItemsMutation.mutate(result.items.map((c) => c.id)),
  })

  const candidates = candidatesQuery.data?.items ?? []

  function toggleSelected(id: string) {
    setSelected((prev) => {
      const next = new Set(prev)
      if (next.has(id)) next.delete(id)
      else next.add(id)
      return next
    })
  }

  function toggleAll() {
    setSelected((prev) => (prev.size === candidates.length ? new Set() : new Set(candidates.map((c) => c.id))))
  }

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
          <h2 className="text-2xl font-semibold tracking-tight">Calibration List Builder</h2>
          <p className="mt-1 text-sm text-muted-foreground">Filter, sample, or manually pick evaluations to add to this list.</p>
        </div>
        <Button variant="secondary" onClick={() => setShowFilters((v) => !v)}>
          <Filter className="size-4" /> Filter
        </Button>
      </div>

      {showFilters && (
        <Card className="mb-6">
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <label className="flex flex-col gap-1.5 text-sm">
              <span className="font-medium">Date From</span>
              <input
                type="date"
                className={inputClass}
                onChange={(e) => setFilters((f) => ({ ...f, dateFrom: e.target.value ? new Date(e.target.value).toISOString() : undefined }))}
              />
            </label>
            <label className="flex flex-col gap-1.5 text-sm">
              <span className="font-medium">Date To</span>
              <input
                type="date"
                className={inputClass}
                onChange={(e) => setFilters((f) => ({ ...f, dateTo: e.target.value ? new Date(e.target.value).toISOString() : undefined }))}
              />
            </label>
            <label className="flex flex-col gap-1.5 text-sm">
              <span className="font-medium">Group</span>
              <select
                className={inputClass}
                value={filters.groupId ?? ''}
                onChange={(e) => setFilters((f) => ({ ...f, groupId: e.target.value || undefined, teamId: undefined }))}
              >
                <option value="">-- Groups --</option>
                {groupsQuery.data?.map((g) => <option key={g.id} value={g.id}>{g.name}</option>)}
              </select>
            </label>
            <label className="flex flex-col gap-1.5 text-sm">
              <span className="font-medium">Team</span>
              <select
                className={inputClass}
                value={filters.teamId ?? ''}
                onChange={(e) => setFilters((f) => ({ ...f, teamId: e.target.value || undefined }))}
              >
                <option value="">-- Teams --</option>
                {teamsQuery.data?.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
              </select>
            </label>
            <label className="flex flex-col gap-1.5 text-sm">
              <span className="font-medium">Evaluator</span>
              <select
                className={inputClass}
                value={filters.evaluatorId ?? ''}
                onChange={(e) => setFilters((f) => ({ ...f, evaluatorId: e.target.value || undefined }))}
              >
                <option value="">-- Evaluators --</option>
                {evaluatorsQuery.data?.map((u) => <option key={u.id} value={u.id}>{u.displayName}</option>)}
              </select>
            </label>
            <label className="flex flex-col gap-1.5 text-sm">
              <span className="font-medium">Reference</span>
              <input
                className={inputClass}
                placeholder="Enter reference..."
                onChange={(e) => setFilters((f) => ({ ...f, reference: e.target.value || undefined }))}
              />
            </label>
            <label className="flex flex-col gap-1.5 text-sm">
              <span className="font-medium">Scorecard</span>
              <select
                className={inputClass}
                value={filters.scorecardId ?? ''}
                onChange={(e) => setFilters((f) => ({ ...f, scorecardId: e.target.value || undefined }))}
              >
                <option value="">-- Scorecards --</option>
                {scorecardsQuery.data?.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
              </select>
            </label>
            <label className="flex flex-col gap-1.5 text-sm">
              <span className="font-medium">Category</span>
              <select
                className={inputClass}
                value={filters.categoryId ?? ''}
                onChange={(e) => setFilters((f) => ({ ...f, categoryId: e.target.value || undefined }))}
              >
                <option value="">-- Categories --</option>
                {categoriesQuery.data?.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
            </label>
            <label className="flex flex-col gap-1.5 text-sm">
              <span className="font-medium">Score min (%)</span>
              <input
                type="number"
                min={0}
                max={100}
                className={inputClass}
                onChange={(e) => setFilters((f) => ({ ...f, scoreMin: e.target.value ? Number(e.target.value) : undefined }))}
              />
            </label>
            <label className="flex flex-col gap-1.5 text-sm">
              <span className="font-medium">Score max (%)</span>
              <input
                type="number"
                min={0}
                max={100}
                className={inputClass}
                onChange={(e) => setFilters((f) => ({ ...f, scoreMax: e.target.value ? Number(e.target.value) : undefined }))}
              />
            </label>
            <label className="flex flex-col gap-1.5 text-sm">
              <span className="font-medium">No. Records (random sample)</span>
              <input
                type="number"
                min={1}
                value={sampleSize}
                onChange={(e) => setSampleSize(Number(e.target.value))}
                className={inputClass}
              />
            </label>
          </div>

          <div className="mt-5 flex justify-end gap-2">
            <Button
              variant="secondary"
              disabled={randomSampleMutation.isPending}
              onClick={() => randomSampleMutation.mutate()}
            >
              <Shuffle className="size-4" />
              {randomSampleMutation.isPending ? 'Sampling...' : 'Random'}
            </Button>
          </div>
        </Card>
      )}

      {addItemsMutation.isError && (
        <p className="mb-4 text-sm text-status-fail">{(addItemsMutation.error as Error).message}</p>
      )}

      <div className="mb-4 flex items-center justify-between">
        <p className="text-sm text-muted-foreground">{selected.size} selected</p>
        <Button
          disabled={selected.size === 0 || addItemsMutation.isPending}
          onClick={() => addItemsMutation.mutate(Array.from(selected))}
        >
          {addItemsMutation.isPending ? 'Adding...' : `Add ${selected.size || ''} to list`}
        </Button>
      </div>

      {candidatesQuery.isLoading && <p className="text-sm text-muted-foreground">Loading evaluations...</p>}
      {candidatesQuery.isError && (
        <p className="text-sm text-status-fail">Failed to load: {(candidatesQuery.error as Error).message}</p>
      )}

      {!candidatesQuery.isLoading && (
        <div className="overflow-hidden rounded-xl border border-border bg-card shadow-sm">
          <DataTable
            headers={['', 'Employee', 'Evaluator', 'Event date', 'Team', 'Score date', 'Scorecard', 'Category', 'Reference', 'Score']}
            rows={candidates.map((c) => [
              <input
                type="checkbox"
                key="select"
                checked={selected.has(c.id)}
                onChange={() => toggleSelected(c.id)}
                className="size-4 rounded border-input"
              />,
              c.agentName,
              c.evaluatorName,
              c.eventOccurredAt ? new Date(c.eventOccurredAt).toLocaleDateString() : '—',
              c.teamNames.join(', ') || '—',
              c.submittedAt ? new Date(c.submittedAt).toLocaleDateString() : '—',
              c.scorecardName,
              c.categoryName,
              c.reference ?? '—',
              c.totalScore !== null ? `${c.totalScore}%` : '—',
            ])}
          />
          {candidates.length > 0 && (
            <div className="border-t border-border p-3">
              <button onClick={toggleAll} className="text-xs font-medium text-primary hover:underline">
                {selected.size === candidates.length ? 'Deselect all' : 'Select all on this page'}
              </button>
            </div>
          )}
        </div>
      )}
    </>
  )
}
