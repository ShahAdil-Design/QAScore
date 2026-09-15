import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, Wand2 } from 'lucide-react'
import clsx from 'clsx'
import { Card } from '@/components/ui/Card'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { SearchableSelect } from '@/components/ui/SearchableSelect'
import { calibrationApi, type CalibrationAnswerInput, type CalibrationQuestion } from '@/api/calibration'
import { lookupsApi } from '@/api/lookups'
import { usersApi } from '@/api/users'
import { useCurrentUser } from '@/auth/currentUserStore'

// The chosen option's configured value — score is always derived, never typed in.
function scoreFor(question: CalibrationQuestion, answerValue: string | null): number | null {
  if (answerValue === null) return null
  return question.answerOptions.find((o) => o.label === answerValue)?.value ?? null
}

export function CalibrationItemDetailPage() {
  const { itemId } = useParams<{ itemId: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const me = useCurrentUser((s) => s.me)

  const [evaluatorId, setEvaluatorId] = useState('')
  const [ratings, setRatings] = useState<Record<string, CalibrationAnswerInput>>({})
  const [seededFor, setSeededFor] = useState<string | null>(null)

  const detailQuery = useQuery({
    queryKey: ['calibration-item', itemId],
    queryFn: () => calibrationApi.getItemDetail(itemId!),
    enabled: !!itemId,
  })

  const evaluatorsQuery = useQuery({
    queryKey: ['users', 'evaluator-eligible'],
    queryFn: () => usersApi.list(),
    select: (users) => users.filter((u) => u.role !== 'Agent'),
    enabled: !me,
  })

  const causeCodesQuery = useQuery({ queryKey: ['lookups', 'cause-codes'], queryFn: () => lookupsApi.getCauseCodes() })
  const commentsQuery = useQuery({ queryKey: ['lookups', 'comments'], queryFn: () => lookupsApi.getComments() })

  const effectiveEvaluatorId = me?.id ?? evaluatorId

  // Seed the editable ratings once per (item, evaluator) pair — from that evaluator's
  // already-saved rating if one exists, otherwise blank.
  useEffect(() => {
    if (!detailQuery.data || !effectiveEvaluatorId) return
    const seedKey = `${detailQuery.data.id}:${effectiveEvaluatorId}`
    if (seededFor === seedKey) return
    const myRating = detailQuery.data.ratings.find((r) => r.evaluatorId === effectiveEvaluatorId)
    const seeded: Record<string, CalibrationAnswerInput> = {}
    for (const q of detailQuery.data.questions) {
      const existing = myRating?.answers.find((a) => a.questionId === q.questionId)
      seeded[q.questionId] = {
        questionId: q.questionId,
        answerValue: existing?.answerValue ?? null,
        causeCode: existing?.causeCode ?? null,
        comment: existing?.comment ?? null,
      }
    }
    setRatings(seeded)
    setSeededFor(seedKey)
  }, [detailQuery.data, effectiveEvaluatorId, seededFor])

  const saveMutation = useMutation({
    mutationFn: () => calibrationApi.saveAnswers(itemId!, effectiveEvaluatorId, Object.values(ratings)),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['calibration-item', itemId] })
      queryClient.invalidateQueries({ queryKey: ['calibration-items'] })
      queryClient.invalidateQueries({ queryKey: ['calibration-lists'] })
    },
  })

  function updateRating(questionId: string, patch: Partial<CalibrationAnswerInput>) {
    setRatings((prev) => ({ ...prev, [questionId]: { ...prev[questionId], ...patch } }))
  }

  function autoFill() {
    if (!detailQuery.data) return
    setRatings((prev) => {
      const next = { ...prev }
      for (const q of detailQuery.data!.questions) {
        next[q.questionId] = {
          questionId: q.questionId,
          answerValue: q.originalAnswerValue,
          causeCode: q.originalCauseCode,
          comment: q.originalComment,
        }
      }
      return next
    })
  }

  if (detailQuery.isLoading) return <p className="text-sm text-muted-foreground">Loading...</p>
  if (detailQuery.isError) return <p className="text-sm text-status-fail">Failed to load: {(detailQuery.error as Error).message}</p>

  const item = detailQuery.data!
  const answeredCount = Object.values(ratings).filter((r) => r.answerValue !== null).length
  const totalWeight = item.questions.reduce((sum, q) => sum + q.weight, 0)
  const weightedScore = item.questions.reduce(
    (sum, q) => sum + (scoreFor(q, ratings[q.questionId]?.answerValue ?? null) ?? 0) * q.weight,
    0,
  )
  const pct = totalWeight > 0 ? Math.round(weightedScore / totalWeight) : 0

  return (
    <>
      <button
        onClick={() => navigate(`/calibration/${item.calibrationListId}/items`)}
        className="mb-4 flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
      >
        <ArrowLeft className="size-4" /> Back to items
      </button>

      <div className="mb-6 flex flex-wrap items-start justify-between gap-4">
        <div>
          <h2 className="text-2xl font-semibold tracking-tight">Calibrate Performance for {item.agentName}</h2>
          <p className="mt-1 text-sm text-muted-foreground">{item.scorecardName}</p>
        </div>
        {item.variance !== null && (
          <Badge tone={item.variance > 10 ? 'fail' : 'pass'}>Variance: {item.variance}</Badge>
        )}
      </div>

      <Card className="mb-6">
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <Field label="Employee" value={item.agentName} />
          <Field label="Evaluator" value={item.evaluatorName} />
          <Field label="Team" value={item.teamNames.join(', ') || '—'} />
          <Field label="Event Type" value={item.eventTypeName ?? '—'} />
          <Field label="Sub Type" value={item.eventSubTypeName ?? '—'} />
          <Field label="Event Date" value={item.eventOccurredAt ? new Date(item.eventOccurredAt).toLocaleString() : '—'} />
          <Field
            label="Event Duration"
            value={item.eventDurationSeconds != null ? `${Math.floor(item.eventDurationSeconds / 60)}m ${item.eventDurationSeconds % 60}s` : '—'}
          />
          <Field label="Reference" value={item.reference ?? '—'} />
          <Field label="Original Score" value={item.originalScore !== null ? `${item.originalScore}%` : '—'} />
        </div>
      </Card>

      {!me && (
        <Card className="mb-6">
          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">Calibrating as</span>
            <select
              value={evaluatorId}
              onChange={(e) => setEvaluatorId(e.target.value)}
              className="rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring"
            >
              <option value="" disabled>Select who you're calibrating as...</option>
              {evaluatorsQuery.data?.map((u) => <option key={u.id} value={u.id}>{u.displayName}</option>)}
            </select>
          </label>
        </Card>
      )}

      {item.ratings.length > 0 && (
        <Card className="mb-6">
          <h3 className="mb-3 font-semibold">Calibration ratings so far</h3>
          <div className="flex flex-col gap-2">
            {item.ratings.map((r) => (
              <div key={r.evaluatorId} className="flex items-center justify-between rounded-lg border border-border p-3 text-sm">
                <span className="font-medium">{r.evaluatorName}</span>
                <span className="font-semibold text-primary">{r.score !== null ? `${r.score}%` : 'In progress'}</span>
              </div>
            ))}
          </div>
        </Card>
      )}

      {effectiveEvaluatorId && (
        <>
          <div className="mb-4 flex items-center justify-between">
            <div>
              <h3 className="font-semibold">Questions</h3>
              <p className="mt-1 text-sm text-muted-foreground">{answeredCount} of {item.questions.length} answered</p>
            </div>
            <div className="flex items-center gap-3">
              <Button variant="secondary" onClick={autoFill}>
                <Wand2 className="size-4" /> Auto Fill
              </Button>
              <span className="text-2xl font-semibold text-primary">{pct}%</span>
            </div>
          </div>

          <div className="flex flex-col gap-3">
            {item.questions.map((q, i) => {
              const myScore = scoreFor(q, ratings[q.questionId]?.answerValue ?? null)
              return (
                <div key={q.questionId} className="rounded-xl border border-border p-4">
                  <div className="flex items-start gap-3">
                    <span className="flex size-7 shrink-0 items-center justify-center rounded-full bg-muted text-xs font-medium text-muted-foreground">
                      {i + 1}
                    </span>
                    <div>
                      <p className="text-sm font-medium">{q.questionText}</p>
                      <p className="text-xs text-muted-foreground">{q.sectionName} · Weight {q.weight}%</p>
                    </div>
                  </div>

                  <div className="mt-4 grid gap-4 sm:grid-cols-2">
                    <div className="rounded-lg bg-muted/40 p-3">
                      <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Original (evaluator)</p>
                      <p className="mt-2 text-sm">
                        Answer: <span className="font-medium">{q.originalAnswerValue ?? '—'}</span>
                        {q.originalScore !== null && <span className="ml-2 font-semibold text-primary">{q.originalScore}</span>}
                      </p>
                      {q.originalCauseCode && <p className="mt-1 text-xs text-muted-foreground">Cause: {q.originalCauseCode}</p>}
                      {q.originalComment && <p className="mt-1 text-xs text-muted-foreground">Comment: {q.originalComment}</p>}
                    </div>

                    <div className="rounded-lg border border-border p-3">
                      <div className="flex items-center justify-between">
                        <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Your rating</p>
                        {/* Score is derived from the chosen answer below — never typed in directly. */}
                        <span className="text-sm font-semibold text-primary" aria-label={`Your score for ${q.questionText}`}>
                          {myScore ?? '—'}
                        </span>
                      </div>
                      <div className="mt-2 flex flex-wrap gap-2">
                        {q.answerOptions.map((option) => (
                          <button
                            key={option.label}
                            type="button"
                            onClick={() => updateRating(q.questionId, { answerValue: option.label })}
                            title={option.isFailAll ? 'Fails the whole evaluation' : option.isFailSection ? 'Fails this section' : undefined}
                            className={clsx(
                              'rounded-lg border px-2.5 py-1 text-xs font-medium transition-colors',
                              ratings[q.questionId]?.answerValue === option.label
                                ? 'border-primary bg-primary/10 text-primary'
                                : 'border-border text-muted-foreground hover:border-primary/50 hover:text-foreground',
                              (option.isFailAll || option.isFailSection) && 'border-status-fail/50 text-status-fail',
                            )}
                          >
                            {option.label}
                          </button>
                        ))}
                      </div>
                      <div className="mt-3 grid gap-2">
                        <SearchableSelect
                          value={ratings[q.questionId]?.causeCode ?? null}
                          options={causeCodesQuery.data ?? []}
                          placeholder="Select cause code..."
                          emptyLabel="No cause codes configured yet"
                          onChange={(text) => updateRating(q.questionId, { causeCode: text })}
                        />
                        <SearchableSelect
                          value={ratings[q.questionId]?.comment ?? null}
                          options={commentsQuery.data ?? []}
                          placeholder="Select comment..."
                          emptyLabel="No canned comments configured yet"
                          onChange={(text) => updateRating(q.questionId, { comment: text })}
                        />
                      </div>
                    </div>
                  </div>
                </div>
              )
            })}
          </div>

          {saveMutation.isError && (
            <p className="mt-3 text-sm text-status-fail">{(saveMutation.error as Error).message}</p>
          )}

          <div className="mt-4">
            <Button disabled={saveMutation.isPending} onClick={() => saveMutation.mutate()}>
              {saveMutation.isPending ? 'Saving...' : 'Save Score'}
            </Button>
          </div>
        </>
      )}
    </>
  )
}

function Field({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="text-xs font-medium text-muted-foreground">{label}</p>
      <p className="mt-1 rounded-lg bg-muted/50 px-3 py-2 text-sm">{value}</p>
    </div>
  )
}
