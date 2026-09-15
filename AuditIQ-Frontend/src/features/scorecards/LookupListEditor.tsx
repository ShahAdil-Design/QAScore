import { useState } from 'react'
import { Check, Pencil, Plus, Trash2, X } from 'lucide-react'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import type { LookupItem } from '@/api/lookups'

interface LookupListEditorProps {
  title: string
  description: string
  items: LookupItem[]
  isLoading: boolean
  error: Error | null
  onCreate: (text: string) => void
  onUpdate: (id: string, text: string) => void
  onDelete: (id: string) => void
  mutationError: Error | null
  readOnly?: boolean
}

export function LookupListEditor({
  title, description, items, isLoading, error, onCreate, onUpdate, onDelete, mutationError, readOnly = false,
}: LookupListEditorProps) {
  const [newText, setNewText] = useState('')
  const [editingId, setEditingId] = useState<string | null>(null)
  const [editingText, setEditingText] = useState('')

  const inputClass = 'w-full rounded-lg border border-input bg-background px-3 py-1.5 text-sm outline-none focus:ring-2 focus:ring-ring'

  function startEdit(item: LookupItem) {
    setEditingId(item.id)
    setEditingText(item.text)
  }

  function submitNew(e: React.FormEvent) {
    e.preventDefault()
    if (!newText.trim()) return
    onCreate(newText.trim())
    setNewText('')
  }

  function submitEdit() {
    if (!editingId || !editingText.trim()) return
    onUpdate(editingId, editingText.trim())
    setEditingId(null)
  }

  return (
    <Card>
      <h3 className="font-semibold">{title}</h3>
      <p className="mt-1 text-sm text-muted-foreground">{description}</p>

      {!readOnly && (
        <form onSubmit={submitNew} className="mt-4 flex gap-2">
          <input
            className={inputClass}
            value={newText}
            onChange={(e) => setNewText(e.target.value)}
            placeholder={`Add a new ${title.toLowerCase().replace(/s$/, '')}...`}
          />
          <Button type="submit" disabled={!newText.trim()}>
            <Plus className="size-4" />
          </Button>
        </form>
      )}
      {mutationError && <p className="mt-2 text-sm text-status-fail">{mutationError.message}</p>}

      {isLoading && <p className="mt-4 text-sm text-muted-foreground">Loading...</p>}
      {error && <p className="mt-4 text-sm text-status-fail">Failed to load: {error.message}</p>}

      {!isLoading && (
        <div className="mt-4 flex max-h-80 flex-col gap-2 overflow-y-auto">
          {items.length === 0 && <p className="text-sm text-muted-foreground">None configured yet.</p>}
          {items.map((item) => (
            <div key={item.id} className="flex items-center gap-2 rounded-lg border border-border p-2">
              {editingId === item.id ? (
                <>
                  <input
                    autoFocus
                    className={inputClass}
                    value={editingText}
                    onChange={(e) => setEditingText(e.target.value)}
                    onKeyDown={(e) => e.key === 'Enter' && submitEdit()}
                  />
                  <button onClick={submitEdit} className="text-status-pass hover:opacity-80">
                    <Check className="size-4" />
                  </button>
                  <button onClick={() => setEditingId(null)} className="text-muted-foreground hover:text-foreground">
                    <X className="size-4" />
                  </button>
                </>
              ) : (
                <>
                  <span className="flex-1 text-sm">{item.text}</span>
                  {!readOnly && (
                    <>
                      <button onClick={() => startEdit(item)} className="text-muted-foreground hover:text-foreground">
                        <Pencil className="size-4" />
                      </button>
                      <button onClick={() => onDelete(item.id)} className="text-muted-foreground hover:text-status-fail">
                        <Trash2 className="size-4" />
                      </button>
                    </>
                  )}
                </>
              )}
            </div>
          ))}
        </div>
      )}
    </Card>
  )
}
