import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Plus, Trash2, X } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { groupsApi } from '@/api/groups'
import type { ScorecardCategory } from '@/api/scorecardCategories'
import type { AnswerOptionInput, ScorecardQuestionInput, ScorecardWriteInput } from '@/api/scorecards'

interface DraftQuestion extends Omit<ScorecardQuestionInput, 'answerOptions'> {
  answerOptions: AnswerOptionInput[]
}

function emptyOption(): AnswerOptionInput {
  return { label: '', value: 0, isFailSection: false, isFailAll: false, isNotApplicable: false }
}

function emptyQuestion(sortOrder: number): DraftQuestion {
  return { sectionName: '', text: '', weight: 10, isFailLogic: false, sortOrder, answerOptions: [] }
}

// Common answer sets, several pulled directly from real migrated Scorebuddy scorecards — quick
// picks so you don't have to build the answer list by hand every time. Each maps to sensible
// default Values (a clean Pass = 100, a hard Fail = 0); edit any row afterward as needed.
const answerPresets: { label: string; options: AnswerOptionInput[] }[] = [
  {
    label: 'Pass, Fail',
    options: [
      { label: 'Pass', value: 100, isFailSection: false, isFailAll: false, isNotApplicable: false },
      { label: 'Fail', value: 0, isFailSection: false, isFailAll: false, isNotApplicable: false },
    ],
  },
  {
    label: 'Pass, Fail, N/A',
    options: [
      { label: 'Pass', value: 100, isFailSection: false, isFailAll: false, isNotApplicable: false },
      { label: 'Fail', value: 0, isFailSection: false, isFailAll: false, isNotApplicable: false },
      { label: 'N/A', value: 0, isFailSection: false, isFailAll: false, isNotApplicable: true },
    ],
  },
  {
    label: 'Pass, Requires Improvement, Fail',
    options: [
      { label: 'Pass', value: 100, isFailSection: false, isFailAll: false, isNotApplicable: false },
      { label: 'Requires Improvement', value: 50, isFailSection: false, isFailAll: false, isNotApplicable: false },
      { label: 'Fail', value: 0, isFailSection: false, isFailAll: false, isNotApplicable: false },
    ],
  },
  {
    label: 'Pass, Procedural Fail, Breach',
    options: [
      { label: 'Pass', value: 100, isFailSection: false, isFailAll: false, isNotApplicable: false },
      { label: 'Procedural Fail', value: 0, isFailSection: true, isFailAll: false, isNotApplicable: false },
      { label: 'Breach', value: 0, isFailSection: false, isFailAll: true, isNotApplicable: false },
    ],
  },
]

interface ScorecardBuilderFormProps {
  categories: ScorecardCategory[]
  initial?: {
    name: string
    description: string | null
    scorecardType: string
    categoryId: string
    location: string | null
    targetPercentage: number | null
    maxScore: number
    groupIds: string[]
    questions: DraftQuestion[]
  }
  submitLabel: string
  onSubmit: (input: ScorecardWriteInput) => void
  onCancel: () => void
  isSubmitting: boolean
}

export function ScorecardBuilderForm({ categories, initial, submitLabel, onSubmit, onCancel, isSubmitting }: ScorecardBuilderFormProps) {
  const [name, setName] = useState(initial?.name ?? '')
  const [description, setDescription] = useState(initial?.description ?? '')
  const [scorecardType, setScorecardType] = useState(initial?.scorecardType ?? 'Standard')
  const [categoryId, setCategoryId] = useState(initial?.categoryId ?? categories[0]?.id ?? '')
  const [location, setLocation] = useState(initial?.location ?? '')
  const [targetPercentage, setTargetPercentage] = useState(
    initial?.targetPercentage != null ? String(initial.targetPercentage) : '',
  )
  const [maxScore, setMaxScore] = useState(String(initial?.maxScore ?? 100))
  const [groupIds, setGroupIds] = useState<string[]>(initial?.groupIds ?? [])
  const [questions, setQuestions] = useState<DraftQuestion[]>(initial?.questions ?? [])

  const groupsQuery = useQuery({ queryKey: ['groups'], queryFn: () => groupsApi.list() })

  function updateQuestion(index: number, patch: Partial<DraftQuestion>) {
    setQuestions((prev) => prev.map((q, i) => (i === index ? { ...q, ...patch } : q)))
  }

  function addQuestion() {
    setQuestions((prev) => [...prev, emptyQuestion(prev.length)])
  }

  function removeQuestion(index: number) {
    setQuestions((prev) => prev.filter((_, i) => i !== index).map((q, i) => ({ ...q, sortOrder: i })))
  }

  function updateOption(qIndex: number, oIndex: number, patch: Partial<AnswerOptionInput>) {
    setQuestions((prev) =>
      prev.map((q, i) =>
        i === qIndex ? { ...q, answerOptions: q.answerOptions.map((o, j) => (j === oIndex ? { ...o, ...patch } : o)) } : q,
      ),
    )
  }

  function addOption(qIndex: number) {
    setQuestions((prev) => prev.map((q, i) => (i === qIndex ? { ...q, answerOptions: [...q.answerOptions, emptyOption()] } : q)))
  }

  function removeOption(qIndex: number, oIndex: number) {
    setQuestions((prev) =>
      prev.map((q, i) => (i === qIndex ? { ...q, answerOptions: q.answerOptions.filter((_, j) => j !== oIndex) } : q)),
    )
  }

  function applyPreset(qIndex: number, options: AnswerOptionInput[]) {
    updateQuestion(qIndex, { answerOptions: options.map((o) => ({ ...o })) })
  }

  function toggleGroup(groupId: string) {
    setGroupIds((prev) => (prev.includes(groupId) ? prev.filter((g) => g !== groupId) : [...prev, groupId]))
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    onSubmit({
      name,
      description: description || null,
      scorecardType,
      categoryId,
      location: location || null,
      targetPercentage: targetPercentage.trim() ? Number(targetPercentage) : null,
      maxScore: Number(maxScore),
      groupIds,
      questions: questions.map((q) => ({
        sectionName: q.sectionName,
        text: q.text,
        weight: q.weight,
        isFailLogic: q.isFailLogic,
        sortOrder: q.sortOrder,
        answerOptions: q.answerOptions,
      })),
    })
  }

  const inputClass = 'w-full rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring'

  return (
    <Card>
      <form onSubmit={handleSubmit} className="flex flex-col gap-5">
        <div className="grid gap-4 md:grid-cols-2">
          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">Name</span>
            <input className={inputClass} value={name} onChange={(e) => setName(e.target.value)} required />
          </label>
          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">Category</span>
            <select className={inputClass} value={categoryId} onChange={(e) => setCategoryId(e.target.value)} required>
              {categories.map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">Type</span>
            <input className={inputClass} value={scorecardType} onChange={(e) => setScorecardType(e.target.value)} required />
          </label>
          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">Location</span>
            <input className={inputClass} value={location} onChange={(e) => setLocation(e.target.value)} />
          </label>
          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">Target score (%, optional)</span>
            <input
              type="number"
              min="0"
              max="100"
              step="0.01"
              className={inputClass}
              value={targetPercentage}
              onChange={(e) => setTargetPercentage(e.target.value)}
              placeholder="e.g. 85"
            />
          </label>
          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">Max score</span>
            <input
              type="number"
              min="0.01"
              step="0.01"
              className={inputClass}
              value={maxScore}
              onChange={(e) => setMaxScore(e.target.value)}
              required
            />
            <span className="text-xs text-muted-foreground">
              The denominator for the total-score percentage: (answer value × weighting) summed across
              questions, divided by this. Usually matches the sum of question weightings, but doesn't have to.
            </span>
          </label>
          <label className="flex flex-col gap-1.5 text-sm md:col-span-2">
            <span className="font-medium">Description</span>
            <input className={inputClass} value={description} onChange={(e) => setDescription(e.target.value)} />
          </label>

          <div className="flex flex-col gap-1.5 text-sm md:col-span-2">
            <span className="font-medium">Groups</span>
            <div className="flex flex-wrap gap-2">
              {groupsQuery.data?.length ? (
                groupsQuery.data.map((g) => (
                  <button
                    type="button"
                    key={g.id}
                    onClick={() => toggleGroup(g.id)}
                    className={
                      'rounded-lg border px-3 py-1.5 text-xs font-medium transition-colors ' +
                      (groupIds.includes(g.id)
                        ? 'border-primary bg-primary/10 text-primary'
                        : 'border-border text-muted-foreground hover:border-primary/50 hover:text-foreground')
                    }
                  >
                    {g.name}
                  </button>
                ))
              ) : (
                <span className="text-xs text-muted-foreground">No groups configured yet.</span>
              )}
            </div>
            <span className="text-xs text-muted-foreground">
              {groupIds.length === 0 ? 'Visible to All Groups (none selected).' : `Restricted to ${groupIds.length} group(s).`}
            </span>
          </div>
        </div>

        <div className="flex flex-col gap-3">
          <div className="flex items-center justify-between">
            <h4 className="font-semibold">Questions</h4>
            <Button type="button" variant="secondary" onClick={addQuestion}>
              <Plus className="size-4" /> Add question
            </Button>
          </div>

          {questions.length === 0 && (
            <p className="rounded-lg border border-dashed border-border p-4 text-center text-sm text-muted-foreground">
              No questions yet — add one whenever you're ready. A scorecard can be saved as a shell first.
            </p>
          )}

          {questions.map((q, i) => (
            <div key={i} className="rounded-lg border border-border p-4">
              <div className="mb-3 flex items-center justify-between">
                <span className="text-sm font-medium text-muted-foreground">Question {i + 1}</span>
                <button type="button" onClick={() => removeQuestion(i)} className="text-muted-foreground hover:text-status-fail">
                  <Trash2 className="size-4" />
                </button>
              </div>
              <div className="grid gap-3 md:grid-cols-[1fr_1fr_100px]">
                <label className="flex flex-col gap-1.5 text-sm">
                  <span>Section</span>
                  <input className={inputClass} value={q.sectionName} onChange={(e) => updateQuestion(i, { sectionName: e.target.value })} required />
                </label>
                <label className="flex flex-col gap-1.5 text-sm">
                  <span>Question text</span>
                  <input className={inputClass} value={q.text} onChange={(e) => updateQuestion(i, { text: e.target.value })} required />
                </label>
                <label className="flex flex-col gap-1.5 text-sm">
                  <span>Weighting</span>
                  <input type="number" min={0} className={inputClass} value={q.weight} onChange={(e) => updateQuestion(i, { weight: Number(e.target.value) })} required />
                </label>
              </div>
              <p className="mt-1 text-xs text-muted-foreground">
                Weighting is relative, not a percentage — it doesn't need to add up to 100 across questions. The
                score is weighted automatically: (answer value × weighting) summed, divided by total weighting.
              </p>

              <div className="mt-3 flex flex-col gap-2">
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium">Answers</span>
                  <div className="flex flex-wrap gap-1.5">
                    {answerPresets.map((preset) => (
                      <button
                        type="button"
                        key={preset.label}
                        onClick={() => applyPreset(i, preset.options)}
                        className="rounded-lg border border-border px-2 py-1 text-[11px] font-medium text-muted-foreground transition-colors hover:border-primary/50 hover:text-foreground"
                      >
                        {preset.label}
                      </button>
                    ))}
                  </div>
                </div>

                {q.answerOptions.length === 0 && (
                  <p className="rounded-lg border border-dashed border-border p-3 text-center text-xs text-muted-foreground">
                    No answers yet — pick a preset above or add one manually.
                  </p>
                )}

                {q.answerOptions.length > 0 && (
                  <div className="overflow-x-auto">
                    <table className="w-full min-w-[560px] text-left text-xs">
                      <thead className="text-muted-foreground">
                        <tr>
                          <th className="pb-1 pr-2 font-medium">Answer</th>
                          <th className="pb-1 px-2 font-medium">Value</th>
                          <th className="pb-1 px-2 font-medium">Fail Section</th>
                          <th className="pb-1 px-2 font-medium">Fail All</th>
                          <th className="pb-1 px-2 font-medium">N/A</th>
                          <th className="pb-1 pl-2" />
                        </tr>
                      </thead>
                      <tbody>
                        {q.answerOptions.map((option, oi) => (
                          <tr key={oi} className="border-t border-border">
                            <td className="py-1.5 pr-2">
                              <input
                                className={inputClass}
                                value={option.label}
                                onChange={(e) => updateOption(i, oi, { label: e.target.value })}
                                placeholder="Label"
                                required
                              />
                            </td>
                            <td className="py-1.5 px-2">
                              <input
                                type="number"
                                className={inputClass + ' w-20'}
                                value={option.value}
                                disabled={option.isFailSection || option.isFailAll || option.isNotApplicable}
                                onChange={(e) => updateOption(i, oi, { value: Number(e.target.value) })}
                              />
                            </td>
                            <td className="px-2 text-center">
                              <input
                                type="checkbox"
                                checked={option.isFailSection}
                                onChange={(e) => updateOption(i, oi, { isFailSection: e.target.checked, value: e.target.checked ? 0 : option.value })}
                              />
                            </td>
                            <td className="px-2 text-center">
                              <input
                                type="checkbox"
                                checked={option.isFailAll}
                                onChange={(e) => updateOption(i, oi, { isFailAll: e.target.checked, value: e.target.checked ? 0 : option.value })}
                              />
                            </td>
                            <td className="px-2 text-center">
                              <input
                                type="checkbox"
                                checked={option.isNotApplicable}
                                onChange={(e) => updateOption(i, oi, { isNotApplicable: e.target.checked, value: e.target.checked ? 0 : option.value })}
                              />
                            </td>
                            <td className="py-1.5 pl-2 text-right">
                              <button type="button" onClick={() => removeOption(i, oi)} className="text-muted-foreground hover:text-status-fail">
                                <Trash2 className="size-3.5" />
                              </button>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                    <p className="mt-1 text-[11px] text-muted-foreground">
                      Max score for this question: {q.answerOptions.length > 0 ? Math.max(...q.answerOptions.map((o) => o.value)) : 0}.
                      A Fail Section, Fail All, or N/A answer must have a value of zero.
                    </p>
                  </div>
                )}

                <Button type="button" variant="secondary" className="h-7 w-fit px-2 text-xs" onClick={() => addOption(i)}>
                  <Plus className="size-3" /> Add answer
                </Button>
              </div>
            </div>
          ))}
        </div>

        <div className="flex justify-end gap-2">
          <Button type="button" variant="secondary" onClick={onCancel}>
            <X className="size-4" /> Cancel
          </Button>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Saving...' : submitLabel}
          </Button>
        </div>
      </form>
    </Card>
  )
}
