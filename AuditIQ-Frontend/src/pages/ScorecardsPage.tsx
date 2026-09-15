import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { BookOpenCheck, Lock, LockOpen, Pencil, Plus, Search, Archive as ArchiveIcon } from 'lucide-react'
import clsx from 'clsx'
import { SectionHeader } from '@/components/ui/SectionHeader'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { scorecardsApi } from '@/api/scorecards'
import { scorecardCategoriesApi } from '@/api/scorecardCategories'
import { lookupsApi } from '@/api/lookups'
import { NewScorecardModal } from '@/features/scorecards/NewScorecardModal'
import { LookupListEditor } from '@/features/scorecards/LookupListEditor'
import { useCurrentUser } from '@/auth/currentUserStore'

export function ScorecardsPage() {
  const navigate = useNavigate()
  const [isCreating, setIsCreating] = useState(false)
  const [activeTab, setActiveTab] = useState<'scorecards' | 'lookups'>('scorecards')
  const [search, setSearch] = useState('')
  const queryClient = useQueryClient()
  const me = useCurrentUser((s) => s.me)
  // An unresolved identity (legacy plain dev-login, no email claim) keeps full access — only a
  // real resolved non-Admin role actually restricts anything.
  const isAdmin = !me || me.role === 'Admin'

  const scorecardsQuery = useQuery({ queryKey: ['scorecards'], queryFn: () => scorecardsApi.list() })
  const categoriesQuery = useQuery({ queryKey: ['scorecard-categories'], queryFn: () => scorecardCategoriesApi.list() })

  const causeCodesQuery = useQuery({ queryKey: ['lookups', 'cause-codes'], queryFn: () => lookupsApi.getCauseCodes() })
  const commentsQuery = useQuery({ queryKey: ['lookups', 'comments'], queryFn: () => lookupsApi.getComments() })

  const createCauseCodeMutation = useMutation({
    mutationFn: (text: string) => lookupsApi.createCauseCode(text),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['lookups', 'cause-codes'] }),
  })
  const updateCauseCodeMutation = useMutation({
    mutationFn: ({ id, text }: { id: string; text: string }) => lookupsApi.updateCauseCode(id, text),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['lookups', 'cause-codes'] }),
  })
  const deleteCauseCodeMutation = useMutation({
    mutationFn: (id: string) => lookupsApi.deleteCauseCode(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['lookups', 'cause-codes'] }),
  })

  const createCommentMutation = useMutation({
    mutationFn: (text: string) => lookupsApi.createComment(text),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['lookups', 'comments'] }),
  })
  const updateCommentMutation = useMutation({
    mutationFn: ({ id, text }: { id: string; text: string }) => lookupsApi.updateComment(id, text),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['lookups', 'comments'] }),
  })
  const deleteCommentMutation = useMutation({
    mutationFn: (id: string) => lookupsApi.deleteComment(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['lookups', 'comments'] }),
  })

  const createMutation = useMutation({
    mutationFn: (input: { name: string; scorecardType: string; categoryId: string; location: string | null }) =>
      scorecardsApi.create({ ...input, description: null, targetPercentage: null, maxScore: 100, groupIds: [], questions: [] }),
    onSuccess: (result) => {
      queryClient.invalidateQueries({ queryKey: ['scorecards'] })
      setIsCreating(false)
      navigate(`/scorecards/${result.id}/edit`)
    },
  })

  const lockMutation = useMutation({
    mutationFn: (id: string) => scorecardsApi.lock(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['scorecards'] }),
  })

  const unlockMutation = useMutation({
    mutationFn: (id: string) => scorecardsApi.unlock(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['scorecards'] }),
  })

  const archiveMutation = useMutation({
    mutationFn: (id: string) => scorecardsApi.archive(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['scorecards'] }),
  })

  const categories = categoriesQuery.data ?? []

  const filteredScorecards = (scorecardsQuery.data ?? []).filter((card) => {
    const q = search.trim().toLowerCase()
    if (!q) return true
    return card.name.toLowerCase().includes(q) || card.categoryName.toLowerCase().includes(q)
  })

  return (
    <>
      <SectionHeader
        title="Scorecards"
        description="Manage the scorecards used across evaluations. Editing always creates a new version — evaluations keep pointing at the version they were scored against."
        action={
          isAdmin && !isCreating && (
            <Button onClick={() => setIsCreating(true)} disabled={categories.length === 0}>
              <Plus className="size-4" /> New scorecard
            </Button>
          )
        }
      />

      <div className="mb-6 flex gap-1 border-b border-border">
        {([
          ['scorecards', 'Scorecards'],
          ['lookups', 'Cause codes & comments'],
        ] as const).map(([key, label]) => (
          <button
            key={key}
            onClick={() => setActiveTab(key)}
            className={clsx(
              'border-b-2 px-4 py-2.5 text-sm font-medium transition-colors',
              activeTab === key
                ? 'border-primary text-primary'
                : 'border-transparent text-muted-foreground hover:text-foreground',
            )}
          >
            {label}
          </button>
        ))}
      </div>

      {activeTab === 'scorecards' && categoriesQuery.isSuccess && categories.length === 0 && !isCreating && (
        <Card className="mb-6 text-sm text-muted-foreground">
          No scorecard categories exist yet. Create one via the API before adding a scorecard.
        </Card>
      )}

      {isCreating && (
        <NewScorecardModal
          categories={categories}
          isSubmitting={createMutation.isPending}
          error={createMutation.isError ? (createMutation.error as Error).message : null}
          onCancel={() => setIsCreating(false)}
          onSubmit={(input) => createMutation.mutate(input)}
        />
      )}

      {activeTab === 'scorecards' && (
        <>
          {scorecardsQuery.isLoading && <p className="text-sm text-muted-foreground">Loading scorecards...</p>}
          {scorecardsQuery.isError && (
            <p className="text-sm text-status-fail">Failed to load scorecards: {(scorecardsQuery.error as Error).message}</p>
          )}

          {scorecardsQuery.isSuccess && scorecardsQuery.data.length === 0 && !isCreating && (
            <Card className="text-sm text-muted-foreground">No scorecards yet. Create the first one above.</Card>
          )}

          {scorecardsQuery.isSuccess && scorecardsQuery.data.length > 0 && (
            <div className="relative mb-4 max-w-sm">
              <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
              <input
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Search scorecards by name or category..."
                className="w-full rounded-lg border border-input bg-background py-2 pl-9 pr-3 text-sm outline-none focus:ring-2 focus:ring-ring"
              />
            </div>
          )}

          {scorecardsQuery.isSuccess && scorecardsQuery.data.length > 0 && filteredScorecards.length === 0 && (
            <Card className="text-sm text-muted-foreground">No scorecards match "{search}".</Card>
          )}

          <div className="grid gap-4 lg:grid-cols-3">
            {filteredScorecards.map((card) => (
              <Card key={card.id}>
                <div className="flex items-start justify-between">
                  <div className="flex size-10 items-center justify-center rounded-lg bg-primary/10 text-primary">
                    <BookOpenCheck className="size-5" />
                  </div>
                  <span className="text-xs text-muted-foreground">{card.categoryName}</span>
                </div>
                <h3 className="mt-5 font-semibold">{card.name}</h3>
                <p className="mt-1 text-sm text-muted-foreground">
                  {card.questionCount} questions · v{card.version}
                  {card.location && <> · {card.location}</>}
                </p>
                <p className="mt-1 text-xs text-muted-foreground">
                  {card.groupNames.length === 0 ? 'All Groups' : card.groupNames.join(', ')}
                </p>
                {isAdmin && (
                  <div className="mt-4 flex flex-wrap gap-2">
                    <Button
                      variant="secondary"
                      onClick={() => navigate(`/scorecards/${card.id}/edit`)}
                    >
                      <Pencil className="size-4" /> Edit
                    </Button>
                    {card.isLocked ? (
                      <Button
                        variant="secondary"
                        onClick={() => unlockMutation.mutate(card.id)}
                        disabled={unlockMutation.isPending}
                      >
                        <LockOpen className="size-4" /> Unlock
                      </Button>
                    ) : (
                      <Button
                        variant="secondary"
                        onClick={() => lockMutation.mutate(card.id)}
                        disabled={lockMutation.isPending}
                      >
                        <Lock className="size-4" /> Lock
                      </Button>
                    )}
                    <Button
                      variant="secondary"
                      onClick={() => archiveMutation.mutate(card.id)}
                      disabled={archiveMutation.isPending}
                    >
                      <ArchiveIcon className="size-4" /> Archive
                    </Button>
                  </div>
                )}
              </Card>
            ))}
          </div>
        </>
      )}

      {activeTab === 'lookups' && (
        <div className="grid gap-6 lg:grid-cols-2">
          <LookupListEditor
            title="Cause codes"
            description="Managed list feeding the scoring form's cause-code dropdown."
            items={causeCodesQuery.data ?? []}
            isLoading={causeCodesQuery.isLoading}
            error={causeCodesQuery.error as Error | null}
            onCreate={(text) => createCauseCodeMutation.mutate(text)}
            onUpdate={(id, text) => updateCauseCodeMutation.mutate({ id, text })}
            onDelete={(id) => deleteCauseCodeMutation.mutate(id)}
            mutationError={(createCauseCodeMutation.error ?? updateCauseCodeMutation.error ?? deleteCauseCodeMutation.error) as Error | null}
            readOnly={!isAdmin}
          />
          <LookupListEditor
            title="Canned comments"
            description="Managed list feeding the scoring form's comment dropdown."
            items={commentsQuery.data ?? []}
            isLoading={commentsQuery.isLoading}
            error={commentsQuery.error as Error | null}
            onCreate={(text) => createCommentMutation.mutate(text)}
            onUpdate={(id, text) => updateCommentMutation.mutate({ id, text })}
            onDelete={(id) => deleteCommentMutation.mutate(id)}
            mutationError={(createCommentMutation.error ?? updateCommentMutation.error ?? deleteCommentMutation.error) as Error | null}
            readOnly={!isAdmin}
          />
        </div>
      )}
    </>
  )
}
