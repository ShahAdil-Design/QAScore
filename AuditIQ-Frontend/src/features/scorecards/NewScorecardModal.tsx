import { useState } from 'react'
import { X } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import type { ScorecardCategory } from '@/api/scorecardCategories'

interface NewScorecardModalProps {
  categories: ScorecardCategory[]
  isSubmitting: boolean
  error: string | null
  onCancel: () => void
  onSubmit: (input: { name: string; scorecardType: string; categoryId: string; location: string | null }) => void
}

// Two-step creation, matching the legacy tool: this modal only asks for the bare minimum
// (Name, Type, Category) to create an empty shell — questions get added afterward on the
// Edit page, rather than forcing the whole question set to be built before it can be saved.
export function NewScorecardModal({ categories, isSubmitting, error, onCancel, onSubmit }: NewScorecardModalProps) {
  const [name, setName] = useState('')
  const [scorecardType, setScorecardType] = useState('Standard')
  const [categoryId, setCategoryId] = useState(categories[0]?.id ?? '')
  const [location, setLocation] = useState('')

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-foreground/20 p-4">
      <div className="w-full max-w-md rounded-xl border border-border bg-card p-6 shadow-lg">
        <div className="flex items-start justify-between">
          <h3 className="text-lg font-semibold">New Scorecard</h3>
          <button onClick={onCancel} className="text-muted-foreground hover:text-foreground">
            <X className="size-4" />
          </button>
        </div>

        <form
          className="mt-5 flex flex-col gap-4"
          onSubmit={(e) => {
            e.preventDefault()
            if (name.trim() && categoryId) onSubmit({ name: name.trim(), scorecardType, categoryId, location: location.trim() || null })
          }}
        >
          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">Scorecard Name</span>
            <input
              autoFocus
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              className="rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring"
            />
          </label>

          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">Scorecard Type</span>
            <input
              value={scorecardType}
              onChange={(e) => setScorecardType(e.target.value)}
              required
              className="rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring"
            />
          </label>

          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">Category</span>
            <select
              value={categoryId}
              onChange={(e) => setCategoryId(e.target.value)}
              required
              className="rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring"
            >
              {categories.map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </label>

          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">Location (optional)</span>
            <input
              value={location}
              onChange={(e) => setLocation(e.target.value)}
              placeholder="e.g. Nottingham"
              className="rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring"
            />
          </label>

          {error && <p className="text-sm text-status-fail">{error}</p>}

          <div className="mt-2 flex justify-end gap-2">
            <Button type="button" variant="secondary" onClick={onCancel}>Cancel</Button>
            <Button type="submit" disabled={!name.trim() || !categoryId || isSubmitting}>
              {isSubmitting ? 'Adding...' : 'Add Scorecard'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  )
}
