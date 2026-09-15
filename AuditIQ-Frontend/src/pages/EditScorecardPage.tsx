import { useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, Lock, LockOpen } from 'lucide-react'
import { Card } from '@/components/ui/Card'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { scorecardsApi, type ScorecardWriteInput } from '@/api/scorecards'
import { scorecardCategoriesApi } from '@/api/scorecardCategories'
import { useBreadcrumb } from '@/lib/breadcrumb'
import { ScorecardBuilderForm } from './ScorecardBuilderForm'

export function EditScorecardPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const setExtra = useBreadcrumb((s) => s.setExtra)

  const detailQuery = useQuery({
    queryKey: ['scorecard', id],
    queryFn: () => scorecardsApi.getById(id!),
    enabled: !!id,
  })
  const categoriesQuery = useQuery({ queryKey: ['scorecard-categories'], queryFn: () => scorecardCategoriesApi.list() })

  useEffect(() => {
    setExtra(detailQuery.data?.name ?? null)
    return () => setExtra(null)
  }, [detailQuery.data?.name, setExtra])

  const updateMutation = useMutation({
    mutationFn: (input: ScorecardWriteInput) => scorecardsApi.update(id!, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['scorecards'] })
      navigate('/scorecards')
    },
  })

  const unlockMutation = useMutation({
    mutationFn: () => scorecardsApi.unlock(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['scorecards'] })
      queryClient.invalidateQueries({ queryKey: ['scorecard', id] })
    },
  })

  const categories = categoriesQuery.data ?? []

  return (
    <>
      <button
        onClick={() => navigate('/scorecards')}
        className="mb-4 flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
      >
        <ArrowLeft className="size-4" /> Back to Scorecards
      </button>

      {detailQuery.isLoading && <Card className="text-sm text-muted-foreground">Loading scorecard...</Card>}
      {detailQuery.isError && (
        <Card className="text-sm text-status-fail">Failed to load: {(detailQuery.error as Error).message}</Card>
      )}

      {detailQuery.data && (
        <>
          <div className="mb-6 flex items-center gap-3">
            <h2 className="text-2xl font-semibold tracking-tight">
              Edit {detailQuery.data.name} <span className="text-lg font-normal text-muted-foreground">(v{detailQuery.data.version})</span>
            </h2>
            <Badge tone={detailQuery.data.isLocked ? 'fail' : 'pass'}>
              {detailQuery.data.isLocked ? <Lock className="size-3" /> : <LockOpen className="size-3" />}
              {detailQuery.data.isLocked ? 'Locked' : 'Unlocked'}
            </Badge>
            {detailQuery.data.isLocked && (
              <Button
                variant="secondary"
                className="h-7 px-2 text-xs"
                onClick={() => unlockMutation.mutate()}
                disabled={unlockMutation.isPending}
              >
                <LockOpen className="size-3" /> Unlock to edit
              </Button>
            )}
          </div>
          {detailQuery.data.isLocked ? (
            <Card className="text-sm text-muted-foreground">
              This scorecard is locked and cannot be edited. Unlock it above to make changes.
            </Card>
          ) : (
            <ScorecardBuilderForm
              categories={categories}
              submitLabel="Save as new version"
              isSubmitting={updateMutation.isPending}
              onCancel={() => navigate('/scorecards')}
              onSubmit={(input) => updateMutation.mutate(input)}
              initial={{
                name: detailQuery.data.name,
                description: detailQuery.data.description,
                scorecardType: detailQuery.data.scorecardType,
                categoryId: categories.find((c) => c.name === detailQuery.data!.categoryName)?.id ?? '',
                location: detailQuery.data.location,
                targetPercentage: detailQuery.data.targetPercentage,
                maxScore: detailQuery.data.maxScore ?? 100,
                groupIds: detailQuery.data.groupIds,
                questions: detailQuery.data.questions.map((q) => ({
                  sectionName: q.sectionName,
                  text: q.text,
                  weight: q.weight,
                  isFailLogic: q.isFailLogic,
                  sortOrder: q.sortOrder,
                  answerOptions: q.answerOptions,
                })),
              }}
            />
          )}
          {updateMutation.isError && (
            <p className="mt-2 text-sm text-status-fail">{(updateMutation.error as Error).message}</p>
          )}
          {unlockMutation.isError && (
            <p className="mt-2 text-sm text-status-fail">{(unlockMutation.error as Error).message}</p>
          )}
        </>
      )}
    </>
  )
}
