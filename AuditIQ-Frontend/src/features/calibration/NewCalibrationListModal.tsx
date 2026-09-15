import { useState } from 'react'
import { X } from 'lucide-react'
import { Button } from '@/components/ui/Button'

const visibilityOptions = ['All Users', 'Supervisors', 'Team Leads', 'Evaluators']

interface NewCalibrationListModalProps {
  isSubmitting: boolean
  error: string | null
  onCancel: () => void
  onSubmit: (input: { name: string; visibilityScope: string }) => void
}

export function NewCalibrationListModal({ isSubmitting, error, onCancel, onSubmit }: NewCalibrationListModalProps) {
  const [name, setName] = useState('')
  const [visibilityScope, setVisibilityScope] = useState(visibilityOptions[0])

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-foreground/20 p-4">
      <div className="w-full max-w-md rounded-xl border border-border bg-card p-6 shadow-lg">
        <div className="flex items-start justify-between">
          <h3 className="text-lg font-semibold">New Calibration List</h3>
          <button onClick={onCancel} className="text-muted-foreground hover:text-foreground">
            <X className="size-4" />
          </button>
        </div>

        <form
          className="mt-5 flex flex-col gap-4"
          onSubmit={(e) => {
            e.preventDefault()
            if (name.trim()) onSubmit({ name: name.trim(), visibilityScope })
          }}
        >
          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">List Name</span>
            <input
              autoFocus
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              className="rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring"
            />
          </label>

          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">User Types</span>
            <select
              value={visibilityScope}
              onChange={(e) => setVisibilityScope(e.target.value)}
              className="rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring"
            >
              {visibilityOptions.map((v) => (
                <option key={v} value={v}>{v}</option>
              ))}
            </select>
            <span className="text-xs text-muted-foreground">Please specify the types of users this list will be visible to.</span>
          </label>

          {error && <p className="text-sm text-status-fail">{error}</p>}

          <div className="mt-2 flex justify-end gap-2">
            <Button type="button" variant="secondary" onClick={onCancel}>Cancel</Button>
            <Button type="submit" disabled={!name.trim() || isSubmitting}>
              {isSubmitting ? 'Creating...' : 'Create List'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  )
}
