import clsx from 'clsx'
import { useQuery } from '@tanstack/react-query'
import type { EvaluationAnswer } from '@/api/evaluations'
import { lookupsApi } from '@/api/lookups'
import { SearchableSelect } from '@/components/ui/SearchableSelect'

interface QuestionCardProps {
  index: number
  answer: EvaluationAnswer
  // A patch, not a full merged object — the caller applies it against its own latest state.
  // Passing a pre-merged `{...answer, x}` here risks dropping a sibling field if two onChange
  // calls fire before React re-renders, since both would close over the same stale `answer` prop.
  onChange: (patch: Partial<EvaluationAnswer>) => void
}

export function QuestionCard({ index, answer, onChange }: QuestionCardProps) {
  const causeCodesQuery = useQuery({ queryKey: ['lookups', 'cause-codes'], queryFn: () => lookupsApi.getCauseCodes() })
  const commentsQuery = useQuery({ queryKey: ['lookups', 'comments'], queryFn: () => lookupsApi.getComments() })

  return (
    <div className="rounded-xl border border-border p-4">
      <div className="flex items-start justify-between gap-3">
        <div className="flex items-start gap-3">
          <span className="flex size-7 shrink-0 items-center justify-center rounded-full bg-muted text-xs font-medium text-muted-foreground">
            {index + 1}
          </span>
          <div>
            <p className="text-sm font-medium">{answer.questionText}</p>
            <p className="text-xs text-muted-foreground">{answer.sectionName} · Weight {answer.weight}%</p>
          </div>
        </div>
        {/* Score is derived from the chosen answer below — never typed in directly. */}
        <span className="shrink-0 text-sm font-semibold text-primary" aria-label={`Score for ${answer.questionText}`}>
          {answer.score ?? '—'}
        </span>
      </div>

      {/* Answer — options vary per question (question_answer_options), never a global Pass/Fail enum.
          Picking one sets both the answer value AND its configured score in the same patch, so the
          score badge above updates immediately without waiting on a save round trip. */}
      <div className="mt-4 flex flex-wrap gap-2">
        {answer.answerOptions.map((option) => (
          <button
            key={option.label}
            type="button"
            onClick={() => onChange({ answerValue: option.label, score: option.value })}
            title={option.isFailAll ? 'Fails the whole evaluation' : option.isFailSection ? 'Fails this section' : undefined}
            className={clsx(
              'rounded-lg border px-3 py-1.5 text-xs font-medium transition-colors',
              answer.answerValue === option.label
                ? 'border-primary bg-primary/10 text-primary'
                : 'border-border text-muted-foreground hover:border-primary/50 hover:text-foreground',
              (option.isFailAll || option.isFailSection) && 'border-status-fail/50 text-status-fail',
            )}
          >
            {option.label}
          </button>
        ))}
      </div>

      <div className="mt-4 grid gap-3 sm:grid-cols-2">
        <label className="text-xs font-medium text-muted-foreground">
          Cause code
          <SearchableSelect
            value={answer.causeCode}
            options={causeCodesQuery.data ?? []}
            placeholder="Select cause code..."
            emptyLabel="No cause codes configured yet"
            onChange={(text) => onChange({ causeCode: text })}
          />
        </label>

        <label className="text-xs font-medium text-muted-foreground">
          Comment
          <SearchableSelect
            value={answer.comment}
            options={commentsQuery.data ?? []}
            placeholder="Select or type a comment..."
            emptyLabel="No canned comments configured yet"
            onChange={(text) => onChange({ comment: text })}
            allowFreeText
          />
        </label>
      </div>
    </div>
  )
}
