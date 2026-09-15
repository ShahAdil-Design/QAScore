import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, MessageSquareWarning, Pencil, ThumbsUp } from 'lucide-react'
import { Card } from '@/components/ui/Card'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { evaluationsApi, type EvaluationStatus } from '@/api/evaluations'
import { useBreadcrumb } from '@/lib/breadcrumb'
import { useCurrentUser } from '@/auth/currentUserStore'

const statusTone: Record<EvaluationStatus, 'pass' | 'fail' | 'warn' | 'neutral'> = {
  Submitted: 'warn',
  Acknowledged: 'pass',
  Disputed: 'fail',
  Resolved: 'neutral',
  Draft: 'neutral',
}

// Read-only for the agent — only their own answers/scores appear here, never the evaluator's
// internal notes (the backend strips that field from this endpoint for the Agent role).
export function EvaluationDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const setExtra = useBreadcrumb((s) => s.setExtra)
  const [disputing, setDisputing] = useState(false)
  const [disputeReason, setDisputeReason] = useState('')
  const [resolutionNotes, setResolutionNotes] = useState('')

  const me = useCurrentUser((s) => s.me)
  // An unresolved identity (legacy dev login) keeps full access, same convention as elsewhere —
  // only a real resolved Agent role is actually restricted from editing/resolving.
  const isAgent = me?.role === 'Agent'

  const detailQuery = useQuery({
    queryKey: ['evaluation', id],
    queryFn: () => evaluationsApi.getById(id!),
    enabled: !!id,
  })

  useEffect(() => {
    setExtra(detailQuery.data?.scorecardName ?? null)
    return () => setExtra(null)
  }, [detailQuery.data?.scorecardName, setExtra])

  const acknowledgeMutation = useMutation({
    mutationFn: () => evaluationsApi.acknowledge(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['evaluation', id] })
      queryClient.invalidateQueries({ queryKey: ['evaluations', 'agent'] })
    },
  })

  const disputeMutation = useMutation({
    mutationFn: () => evaluationsApi.dispute(id!, disputeReason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['evaluation', id] })
      queryClient.invalidateQueries({ queryKey: ['evaluations', 'agent'] })
      setDisputing(false)
      setDisputeReason('')
    },
  })

  const resolveDisputeMutation = useMutation({
    mutationFn: () => evaluationsApi.resolveDispute(id!, resolutionNotes),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['evaluation', id] })
      queryClient.invalidateQueries({ queryKey: ['evaluations', 'agent'] })
      setResolutionNotes('')
    },
  })

  return (
    <>
      <button
        onClick={() => navigate('/review')}
        className="mb-4 flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
      >
        <ArrowLeft className="size-4" /> Back to Review
      </button>

      {detailQuery.isLoading && <Card className="text-sm text-muted-foreground">Loading evaluation...</Card>}
      {detailQuery.isError && (
        <Card className="text-sm text-status-fail">Failed to load: {(detailQuery.error as Error).message}</Card>
      )}

      {detailQuery.data && (
        <>
          <div className="mb-6 flex flex-wrap items-start justify-between gap-4">
            <div>
              <div className="mb-2 flex items-center gap-2">
                <h2 className="text-2xl font-semibold tracking-tight">{detailQuery.data.scorecardName}</h2>
                <Badge tone={statusTone[detailQuery.data.status]}>{detailQuery.data.status}</Badge>
              </div>
              <p className="text-sm text-muted-foreground">
                Evaluator: {detailQuery.data.evaluatorName}
                {detailQuery.data.totalScore !== null && ` · Score: ${detailQuery.data.totalScore}%`}
                {detailQuery.data.reference && ` · Reference: ${detailQuery.data.reference}`}
              </p>
            </div>

            {detailQuery.data.status === 'Submitted' && !disputing && isAgent && (
              <div className="flex gap-2">
                <Button disabled={acknowledgeMutation.isPending} onClick={() => acknowledgeMutation.mutate()}>
                  <ThumbsUp className="size-4" />
                  {acknowledgeMutation.isPending ? 'Acknowledging...' : 'Acknowledge'}
                </Button>
                <Button variant="secondary" onClick={() => setDisputing(true)}>
                  <MessageSquareWarning className="size-4" />
                  Dispute
                </Button>
              </div>
            )}

            {/* Only the evaluator/supervisor corrects a disputed score — never the agent
                being evaluated. */}
            {detailQuery.data.status === 'Disputed' && !isAgent && (
              <Button variant="secondary" onClick={() => navigate(`/score/${id}`)}>
                <Pencil className="size-4" />
                Edit scores
              </Button>
            )}
          </div>

          {acknowledgeMutation.isError && (
            <p className="mb-4 text-sm text-status-fail">{(acknowledgeMutation.error as Error).message}</p>
          )}

          {disputing && (
            <Card className="mb-6">
              <label className="flex flex-col gap-1.5 text-sm">
                <span className="font-medium">Reason for dispute</span>
                <input
                  autoFocus
                  value={disputeReason}
                  onChange={(e) => setDisputeReason(e.target.value)}
                  placeholder="Explain why you're disputing this evaluation..."
                  className="rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring"
                />
              </label>
              {disputeMutation.isError && (
                <p className="mt-2 text-sm text-status-fail">{(disputeMutation.error as Error).message}</p>
              )}
              <div className="mt-4 flex gap-2">
                <Button
                  disabled={!disputeReason || disputeMutation.isPending}
                  onClick={() => disputeMutation.mutate()}
                >
                  {disputeMutation.isPending ? 'Submitting...' : 'Confirm dispute'}
                </Button>
                <Button variant="secondary" onClick={() => { setDisputing(false); setDisputeReason('') }}>
                  Cancel
                </Button>
              </div>
            </Card>
          )}

          <Card>
            <h3 className="mb-4 font-semibold">Questions</h3>
            <div className="flex flex-col gap-3">
              {detailQuery.data.answers.map((a) => (
                <div key={a.questionId} className="rounded-lg border border-border p-3">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="text-sm font-medium">{a.questionText}</p>
                      <p className="text-xs text-muted-foreground">{a.sectionName} · Weight {a.weight}%</p>
                    </div>
                    <span className="shrink-0 text-sm font-semibold text-primary">{a.score ?? '—'}</span>
                  </div>
                  <div className="mt-2 flex flex-wrap gap-3 text-xs text-muted-foreground">
                    <span>Answer: <span className="font-medium text-foreground">{a.answerValue ?? '—'}</span></span>
                    {a.causeCode && <span>Cause: {a.causeCode}</span>}
                    {a.comment && <span>Comment: {a.comment}</span>}
                  </div>
                </div>
              ))}
            </div>
          </Card>

          {detailQuery.data.disputeReason && (
            <Card className="mt-6">
              <h3 className="font-semibold text-status-fail">Dispute reason</h3>
              <p className="mt-2 text-sm text-muted-foreground">{detailQuery.data.disputeReason}</p>
              {detailQuery.data.resolutionNotes && (
                <>
                  <h3 className="mt-4 font-semibold">Resolution</h3>
                  <p className="mt-2 text-sm text-muted-foreground">{detailQuery.data.resolutionNotes}</p>
                </>
              )}

              {detailQuery.data.status === 'Disputed' && !isAgent && (
                <div className="mt-4 border-t border-border pt-4">
                  <label className="flex flex-col gap-1.5 text-sm">
                    <span className="font-medium">Resolve dispute</span>
                    <textarea
                      value={resolutionNotes}
                      onChange={(e) => setResolutionNotes(e.target.value)}
                      placeholder="Explain how this was resolved (e.g. scores corrected after review)..."
                      className="min-h-20 rounded-lg border border-input bg-background p-3 text-sm outline-none focus:ring-2 focus:ring-ring"
                    />
                  </label>
                  {resolveDisputeMutation.isError && (
                    <p className="mt-2 text-sm text-status-fail">{(resolveDisputeMutation.error as Error).message}</p>
                  )}
                  <Button
                    className="mt-3"
                    disabled={!resolutionNotes || resolveDisputeMutation.isPending}
                    onClick={() => resolveDisputeMutation.mutate()}
                  >
                    {resolveDisputeMutation.isPending ? 'Resolving...' : 'Resolve dispute'}
                  </Button>
                </div>
              )}
            </Card>
          )}
        </>
      )}
    </>
  )
}
