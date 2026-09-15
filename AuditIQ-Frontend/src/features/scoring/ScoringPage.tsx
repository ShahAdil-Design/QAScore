import { useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { CheckCircle2, Paperclip, Save } from 'lucide-react'
import clsx from 'clsx'
import { SectionHeader } from '@/components/ui/SectionHeader'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { evaluationsApi, type EvaluationAnswer } from '@/api/evaluations'
import { scorecardsApi } from '@/api/scorecards'
import { usersApi } from '@/api/users'
import { groupsApi } from '@/api/groups'
import { lookupsApi } from '@/api/lookups'
import { SearchableSelect } from '@/components/ui/SearchableSelect'
import { useCurrentUser } from '@/auth/currentUserStore'
import { QuestionCard } from './QuestionCard'

export function ScoringPage() {
  // /score/:evaluationId — reopening a specific evaluation (e.g. a Disputed one an evaluator
  // needs to correct) — takes priority over the "start a new evaluation" flow below.
  const { evaluationId: routeEvaluationId } = useParams<{ evaluationId: string }>()
  const [evaluationId, setEvaluationId] = useState<string | null>(null)

  const activeId = routeEvaluationId ?? evaluationId

  return activeId ? (
    <ActiveEvaluation evaluationId={activeId} />
  ) : (
    <StartEvaluationForm onStarted={setEvaluationId} />
  )
}

function StartEvaluationForm({ onStarted }: { onStarted: (id: string) => void }) {
  const me = useCurrentUser((s) => s.me)
  const [scorecardId, setScorecardId] = useState('')
  const [groupId, setGroupId] = useState('')
  const [agentId, setAgentId] = useState('')
  const [eventTypeId, setEventTypeId] = useState('')
  const [eventSubTypeId, setEventSubTypeId] = useState('')
  const [eventDate, setEventDate] = useState('')
  const [eventTime, setEventTime] = useState('')
  const [eventDurationMinutes, setEventDurationMinutes] = useState('')
  const [reference, setReference] = useState('')

  const scorecardsQuery = useQuery({ queryKey: ['scorecards'], queryFn: () => scorecardsApi.list() })
  const groupsQuery = useQuery({ queryKey: ['groups'], queryFn: () => groupsApi.list() })
  // Matching Scorebuddy's Group -> Employee flow: a group must be picked first (e.g. the "Test
  // Environment" group used to try the product against dummy agents) so the Agent list is
  // scoped to it, rather than showing every agent across every group at once.
  //
  // Not restricted to Role=Agent — a Supervisor's own performance gets evaluated too (by
  // another Supervisor), so anyone in the chosen group is a valid evaluation subject, not just
  // people literally called "Agent" in AuditIQ's role model.
  const agentsQuery = useQuery({
    queryKey: ['users', 'evaluee-eligible', groupId],
    queryFn: () => usersApi.list(undefined, false, groupId),
    enabled: !!groupId,
  })
  // Event types are configured per scorecard in Scorebuddy (two scorecards' "Manage Collections"
  // are unrelated records with their own sub-event lists) — a scorecard with none configured
  // simply has no event type picker, which is expected, not a loading/empty state to fix.
  const eventTypesQuery = useQuery({
    queryKey: ['lookups', 'event-types', scorecardId],
    queryFn: () => lookupsApi.getEventTypes(scorecardId),
    enabled: !!scorecardId,
  })

  const selectedEventType = eventTypesQuery.data?.find((t) => t.id === eventTypeId)

  const createMutation = useMutation({
    mutationFn: () => {
      // Event date + time are captured as separate inputs (matching the legacy tool) but
      // combined into one ISO instant for the API, which only models a single occurredAt.
      const eventOccurredAt = eventDate
        ? new Date(`${eventDate}T${eventTime || '00:00'}`).toISOString()
        : null
      return evaluationsApi.create({
        scorecardId,
        agentId,
        evaluatorId: me!.id,
        eventTypeId: eventTypeId || null,
        eventSubTypeId: eventSubTypeId || null,
        reference: reference || null,
        eventOccurredAt,
        eventDurationSeconds: eventDurationMinutes ? Math.round(Number(eventDurationMinutes) * 60) : null,
      })
    },
    onSuccess: (result) => onStarted(result.id),
  })

  const inputClass = 'w-full rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring'
  const ready = scorecardId && groupId && agentId && !!me

  return (
    <>
      <SectionHeader title="Start an evaluation" description="Pick a scorecard, group, and agent to begin scoring." />
      <Card className="max-w-xl">
        <form
          className="flex flex-col gap-4"
          onSubmit={(e) => {
            e.preventDefault()
            if (ready) createMutation.mutate()
          }}
        >
          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">Scorecard</span>
            <SearchableSelect
              value={scorecardsQuery.data?.find((s) => s.id === scorecardId)?.name ?? null}
              options={(scorecardsQuery.data ?? []).map((s) => ({ id: s.id, text: `${s.name} (v${s.version})` }))}
              placeholder="Select a scorecard..."
              emptyLabel="No scorecards available"
              onChange={(_, id) => {
                setScorecardId(id ?? '')
                setEventTypeId('')
                setEventSubTypeId('')
              }}
            />
          </label>
          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">Group</span>
            <SearchableSelect
              value={groupsQuery.data?.find((g) => g.id === groupId)?.name ?? null}
              options={(groupsQuery.data ?? []).map((g) => ({ id: g.id, text: g.name }))}
              placeholder="Select a group..."
              emptyLabel="No groups available"
              onChange={(_, id) => {
                setGroupId(id ?? '')
                setAgentId('')
              }}
            />
          </label>
          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">Agent</span>
            <SearchableSelect
              value={agentsQuery.data?.find((u) => u.id === agentId)?.displayName ?? null}
              options={(agentsQuery.data ?? []).map((u) => ({ id: u.id, text: u.displayName }))}
              placeholder="Select an agent..."
              emptyLabel={groupId ? 'No agents in this group' : 'Select a group first'}
              onChange={(_, id) => setAgentId(id ?? '')}
            />
          </label>
          <div className="grid grid-cols-2 gap-4">
            <label className="flex flex-col gap-1.5 text-sm">
              <span className="font-medium">Event type</span>
              <SearchableSelect
                value={selectedEventType?.name ?? null}
                options={(eventTypesQuery.data ?? []).map((t) => ({ id: t.id, text: t.name }))}
                placeholder="Select an event type..."
                emptyLabel={scorecardId ? 'No event types configured for this scorecard' : 'Select a scorecard first'}
                onChange={(_, id) => {
                  setEventTypeId(id ?? '')
                  setEventSubTypeId('')
                }}
              />
            </label>
            <label className="flex flex-col gap-1.5 text-sm">
              <span className="font-medium">Sub type</span>
              <SearchableSelect
                value={selectedEventType?.subTypes.find((s) => s.id === eventSubTypeId)?.name ?? null}
                options={(selectedEventType?.subTypes ?? []).map((s) => ({ id: s.id, text: s.name }))}
                placeholder="Select a sub type..."
                emptyLabel={selectedEventType ? 'No sub types configured' : 'Choose an event type first'}
                onChange={(_, id) => setEventSubTypeId(id ?? '')}
              />
            </label>
          </div>

          <div className="grid grid-cols-3 gap-4">
            <label className="flex flex-col gap-1.5 text-sm">
              <span className="font-medium">Event date</span>
              <input type="date" className={inputClass} value={eventDate} onChange={(e) => setEventDate(e.target.value)} />
            </label>
            <label className="flex flex-col gap-1.5 text-sm">
              <span className="font-medium">Event time (optional)</span>
              <input type="time" className={inputClass} value={eventTime} onChange={(e) => setEventTime(e.target.value)} disabled={!eventDate} />
            </label>
            <label className="flex flex-col gap-1.5 text-sm">
              <span className="font-medium">Duration (min)</span>
              <input
                type="number"
                min="0"
                className={inputClass}
                value={eventDurationMinutes}
                onChange={(e) => setEventDurationMinutes(e.target.value)}
              />
            </label>
          </div>

          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">Reference</span>
            <input className={inputClass} value={reference} onChange={(e) => setReference(e.target.value)} />
          </label>

          {createMutation.isError && (
            <p className="text-sm text-status-fail">{(createMutation.error as Error).message}</p>
          )}

          <Button type="submit" disabled={!ready || createMutation.isPending}>
            {createMutation.isPending ? 'Starting...' : 'Start evaluation'}
          </Button>
        </form>
      </Card>
    </>
  )
}

// Groups answers by section while preserving each question's original index (used for the
// numbered badge), so questions render under a visible section header instead of a flat list.
function groupBySection(answers: EvaluationAnswer[]) {
  const sections: { name: string; items: { answer: EvaluationAnswer; index: number }[] }[] = []
  answers.forEach((answer, index) => {
    let section = sections.find((s) => s.name === answer.sectionName)
    if (!section) {
      section = { name: answer.sectionName, items: [] }
      sections.push(section)
    }
    section.items.push({ answer, index })
  })
  return sections
}

function ActiveEvaluation({ evaluationId }: { evaluationId: string }) {
  const queryClient = useQueryClient()
  const evaluationQuery = useQuery({
    queryKey: ['evaluation', evaluationId],
    queryFn: () => evaluationsApi.getById(evaluationId),
  })

  const [answers, setAnswers] = useState<EvaluationAnswer[] | null>(null)
  const [notes, setNotes] = useState('')

  // Seed local editable state once the real evaluation loads.
  if (evaluationQuery.data && answers === null) {
    setAnswers(evaluationQuery.data.answers)
    setNotes(evaluationQuery.data.evaluatorNotes ?? '')
  }

  const saveDraftMutation = useMutation({
    mutationFn: () =>
      evaluationsApi.saveDraftAnswers(
        evaluationId,
        (answers ?? []).map((a) => ({ questionId: a.questionId, answerValue: a.answerValue, causeCode: a.causeCode, comment: a.comment })),
      ),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['evaluation', evaluationId] }),
  })

  const submitMutation = useMutation({
    mutationFn: async () => {
      await evaluationsApi.saveDraftAnswers(
        evaluationId,
        (answers ?? []).map((a) => ({ questionId: a.questionId, answerValue: a.answerValue, causeCode: a.causeCode, comment: a.comment })),
      )
      await evaluationsApi.submit(evaluationId, notes || null)
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['evaluation', evaluationId] }),
  })

  if (evaluationQuery.isLoading || !answers) return <p className="text-sm text-muted-foreground">Loading evaluation...</p>
  if (evaluationQuery.isError) return <p className="text-sm text-status-fail">Failed to load evaluation: {(evaluationQuery.error as Error).message}</p>

  const evaluation = evaluationQuery.data!
  const answered = answers.filter((a) => a.answerValue !== null).length
  const totalWeight = answers.reduce((sum, a) => sum + a.weight, 0)
  const weightedScore = answers.reduce((sum, a) => sum + (a.score ?? 0) * a.weight, 0)
  const pct = totalWeight > 0 ? Math.round(weightedScore / totalWeight) : 0
  const isDraft = evaluation.status === 'Draft'
  const isDisputed = evaluation.status === 'Disputed'
  // Disputed is editable too — the evaluator correcting a disputed score reopens the same
  // scoring UI, saves corrected answers, then finalizes via "Resolve dispute" on the Review page.
  const isEditable = isDraft || isDisputed
  const target = evaluation.scorecardTargetPercentage
  const meetsTarget = target !== null ? pct >= target : null

  function updateAnswer(questionId: string, patch: Partial<EvaluationAnswer>) {
    setAnswers((prev) => prev!.map((a) => (a.questionId === questionId ? { ...a, ...patch } : a)))
  }

  return (
    <>
      <SectionHeader
        eyebrow={`Evaluation ${evaluation.status === 'Draft' ? 'in progress · Draft' : `· ${evaluation.status}`}`}
        title={evaluation.scorecardName}
        description={`Agent: ${evaluation.agentName} · Evaluator: ${evaluation.evaluatorName}${evaluation.reference ? ` · Reference: ${evaluation.reference}` : ''}`}
        action={
          isEditable && (
            <div className="flex gap-2">
              <Button variant="secondary" onClick={() => saveDraftMutation.mutate()} disabled={saveDraftMutation.isPending}>
                <Save className="size-4" />
                {saveDraftMutation.isPending ? 'Saving...' : isDisputed ? 'Save changes' : 'Save draft'}
              </Button>
              {isDraft && (
                <Button onClick={() => submitMutation.mutate()} disabled={submitMutation.isPending}>
                  <CheckCircle2 className="size-4" />
                  {submitMutation.isPending ? 'Submitting...' : 'Submit evaluation'}
                </Button>
              )}
            </div>
          )
        }
      />

      {isDisputed && (
        <p className="mb-4 text-sm text-muted-foreground">
          This evaluation is disputed. Correct the scores below, save your changes, then resolve the dispute from the{' '}
          <Link to={`/review/${evaluation.id}`} className="text-primary underline">Review page</Link>.
        </p>
      )}

      {submitMutation.isError && <p className="mb-4 text-sm text-status-fail">{(submitMutation.error as Error).message}</p>}
      {saveDraftMutation.isError && <p className="mb-4 text-sm text-status-fail">{(saveDraftMutation.error as Error).message}</p>}

      <div className="grid gap-6 lg:grid-cols-[1fr_360px]">
        <Card>
          <div className="mb-5 flex items-center justify-between">
            <div>
              <h3 className="font-semibold">Questions</h3>
              <p className="mt-1 text-sm text-muted-foreground">
                {answered} of {answers.length} questions answered
              </p>
            </div>
            <span className="text-2xl font-semibold text-primary">{pct}%</span>
          </div>

          {target !== null && (
            <div
              className={clsx(
                'mb-5 flex items-center justify-between rounded-lg border px-4 py-2.5 text-sm font-medium',
                meetsTarget
                  ? 'border-status-pass/40 bg-status-pass/10 text-status-pass'
                  : 'border-status-fail/40 bg-status-fail/10 text-status-fail',
              )}
            >
              <span>{meetsTarget ? 'Above Target' : 'Below Target'}</span>
              <span>{pct}% vs {target}% target</span>
            </div>
          )}

          <div className="flex flex-col gap-5">
            {groupBySection(answers).map((section) => (
              <div key={section.name}>
                <h4 className="mb-3 rounded-md bg-muted px-3 py-1.5 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                  {section.name}
                </h4>
                <div className="flex flex-col gap-3">
                  {section.items.map(({ answer, index }) => (
                    <QuestionCard
                      key={answer.questionId}
                      index={index}
                      answer={answer}
                      onChange={(patch) => updateAnswer(answer.questionId, patch)}
                    />
                  ))}
                </div>
              </div>
            ))}
          </div>
        </Card>

        <div className="flex flex-col gap-6">
          <Card>
            <h3 className="font-semibold">Attachments</h3>
            <p className="mt-1 text-sm text-muted-foreground">Evidence linked to this evaluation (Cloud Storage).</p>
            <button
              disabled
              title="Attachment upload isn't implemented on the backend yet"
              className="mt-4 flex w-full cursor-not-allowed items-center justify-center gap-2 rounded-lg border border-dashed border-input py-6 text-sm text-muted-foreground opacity-60"
            >
              <Paperclip className="size-4" />
              Attach a file
            </button>
          </Card>

          <Card>
            <h3 className="font-semibold">Evaluator notes</h3>
            <textarea
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              disabled={!isDraft}
              placeholder="Add notes for this evaluation..."
              className="mt-3 min-h-28 w-full rounded-lg border border-input bg-background p-3 text-sm outline-none focus:ring-2 focus:ring-ring disabled:opacity-60"
            />
          </Card>
        </div>
      </div>
    </>
  )
}
