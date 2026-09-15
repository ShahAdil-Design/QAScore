import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Award, Eye } from 'lucide-react'
import clsx from 'clsx'
import { SectionHeader } from '@/components/ui/SectionHeader'
import { Card } from '@/components/ui/Card'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { DataTable } from '@/components/ui/DataTable'
import { usersApi } from '@/api/users'
import { evaluationsApi, type EvaluationStatus } from '@/api/evaluations'
import { reviewsApi } from '@/api/reviews'
import { useCurrentUser } from '@/auth/currentUserStore'

const statusTone: Record<EvaluationStatus, 'pass' | 'fail' | 'warn' | 'neutral'> = {
  Submitted: 'warn',
  Acknowledged: 'pass',
  Disputed: 'fail',
  Resolved: 'neutral',
  Draft: 'neutral',
}

export function ReviewPage() {
  const navigate = useNavigate()
  const [agentId, setAgentId] = useState('')
  const me = useCurrentUser((s) => s.me)
  // Defaults to "my own evaluations" for anyone who can evaluate (Supervisor/TeamLead/
  // QaEvaluator/Admin) — the common case landing on this page — while staying clearable to
  // "All Evaluators" to browse everyone else's, unlike a derived fallback would allow.
  const [evaluatorId, setEvaluatorId] = useState(() => (me && me.role !== 'Agent' ? me.id : ''))

  const isSelfServiceAgent = me?.role === 'Agent'
  // Not restricted to Role=Agent — a Supervisor can be the subject of an evaluation too (another
  // Supervisor evaluating them), so the picker needs to offer everyone, not just people whose
  // AuditIQ role happens to be literally "Agent".
  const agentsQuery = useQuery({
    queryKey: ['users', 'evaluee-eligible'],
    queryFn: () => usersApi.list(),
    enabled: !isSelfServiceAgent,
  })
  // Anyone who isn't an Agent is a plausible evaluator — same rule ScoringPage.tsx uses for its
  // Evaluator picker, since migrated Scorebuddy staff don't map cleanly onto QaEvaluator alone.
  const evaluatorsQuery = useQuery({
    queryKey: ['users', 'evaluator-eligible'],
    queryFn: () => usersApi.list(),
    select: (users) => users.filter((u) => u.role !== 'Agent'),
    enabled: !isSelfServiceAgent,
  })
  // A real Agent can only ever see their own queue (the backend rejects anything else with
  // 403) — so both pickers below are hidden for that role entirely rather than letting them
  // pick something else and hit a confusing error. Evaluators+ still get the full pickers to
  // browse — either narrowed to one agent, one evaluator, both, or neither ("everyone").
  const effectiveAgentId = isSelfServiceAgent ? me.id : agentId

  const evaluationsQuery = useQuery({
    queryKey: ['evaluations', 'queue', effectiveAgentId, evaluatorId],
    queryFn: () => evaluationsApi.getQueue({
      agentId: effectiveAgentId || undefined,
      evaluatorId: evaluatorId || undefined,
    }),
  })

  const kudosQuery = useQuery({
    queryKey: ['kudos', effectiveAgentId],
    queryFn: () => reviewsApi.getKudos(effectiveAgentId),
    enabled: !!effectiveAgentId,
  })

  const evaluations = evaluationsQuery.data?.items ?? []

  return (
    <>
      <SectionHeader
        title="Employee Review"
        description="Evaluation results, acknowledgments, and disputes (Epic 4) — for yourself, an agent, an evaluator, or everyone."
        action={
          !isSelfServiceAgent && (
            <div className="flex gap-2">
              <select
                className="rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring"
                value={agentId}
                onChange={(e) => setAgentId(e.target.value)}
              >
                <option value="">All Agents</option>
                {agentsQuery.data?.map((u) => (
                  <option key={u.id} value={u.id}>{u.displayName}</option>
                ))}
              </select>
              <select
                className="rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring"
                value={evaluatorId}
                onChange={(e) => setEvaluatorId(e.target.value)}
              >
                <option value="">All Evaluators</option>
                {evaluatorsQuery.data?.map((u) => (
                  <option key={u.id} value={u.id}>{u.displayName}</option>
                ))}
              </select>
            </div>
          )
        }
      />

      <div className={clsx('grid gap-6', effectiveAgentId ? 'lg:grid-cols-[1fr_320px]' : 'lg:grid-cols-1')}>
        <div className="overflow-hidden rounded-xl border border-border bg-card shadow-sm">
          <div className="flex items-center justify-between border-b border-border p-5">
            <div className="text-sm font-medium">Evaluations</div>
          </div>
          {evaluationsQuery.isLoading && <p className="p-5 text-sm text-muted-foreground">Loading...</p>}
          {evaluationsQuery.isError && (
            <p className="p-5 text-sm text-status-fail">Failed to load: {(evaluationsQuery.error as Error).message}</p>
          )}
          {!evaluationsQuery.isLoading && evaluations.length === 0 && (
            <p className="p-5 text-sm text-muted-foreground">No evaluations match this filter.</p>
          )}
          {!evaluationsQuery.isLoading && evaluations.length > 0 && (
            <DataTable
              headers={['Agent', 'Evaluator', 'Scorecard', 'Score', 'Status', 'Actions']}
              rows={evaluations.map((e) => [
                e.agentName,
                e.evaluatorName,
                <span className="font-medium text-foreground" key="scorecard">{e.scorecardName}</span>,
                e.totalScore !== null ? `${e.totalScore}%` : '—',
                <Badge tone={statusTone[e.status]} key="status">{e.status}</Badge>,
                <Button
                  key="actions"
                  variant="secondary"
                  className="h-7 px-2 text-xs"
                  onClick={() => navigate(`/review/${e.id}`)}
                >
                  <Eye className="size-3" />
                  View
                </Button>,
              ])}
            />
          )}
        </div>

        {effectiveAgentId && (
          <Card>
            <div className="flex items-center gap-2 font-semibold">
              <Award className="size-4 text-primary" />
              Kudos
            </div>
            <p className="mt-1 text-sm text-muted-foreground">Recognition from your team.</p>
            <div className="mt-4 flex flex-col gap-4">
              {kudosQuery.data?.length === 0 && <p className="text-sm text-muted-foreground">No kudos yet.</p>}
              {kudosQuery.data?.map((k) => (
                <div key={k.id} className="rounded-lg bg-muted/50 p-3 text-sm">
                  <p className="font-medium">{k.fromUserName}</p>
                  <p className="mt-1 text-muted-foreground">{k.message}</p>
                  <p className="mt-2 text-xs text-muted-foreground">{new Date(k.createdAt).toLocaleDateString()}</p>
                </div>
              ))}
            </div>
          </Card>
        )}
      </div>
    </>
  )
}
