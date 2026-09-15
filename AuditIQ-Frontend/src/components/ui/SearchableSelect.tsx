import { useEffect, useRef, useState } from 'react'
import { Check, ChevronDown, X } from 'lucide-react'
import clsx from 'clsx'

export interface SearchableSelectOption {
  id: string
  text: string
}

interface SearchableSelectProps {
  value: string | null
  options: SearchableSelectOption[]
  placeholder: string
  emptyLabel: string
  // id is additive — free-text pickers (cause code, comment) only need `text`;
  // id-based pickers (selecting a specific record by id) read the second argument.
  onChange: (text: string | null, id?: string) => void
  // When true, typing text that doesn't match any canned option offers a "Use ..." row that
  // commits it directly (no id — it's not one of the lookup rows) instead of forcing a pick
  // from the list. Off by default so id-based pickers (agent/group/scorecard) keep requiring
  // a real match.
  allowFreeText?: boolean
}

/**
 * Replaces a native <select> for long lookup lists (cause codes, canned comments). A native
 * select's popup is sized by the browser to fit its widest option and isn't constrained by page
 * layout at all — with entries that are full sentences, it visibly overflows past neighboring
 * content in both directions. This renders its own panel instead: capped to the trigger's width,
 * capped height with internal scrolling, and a search box since scrolling a plain list of 100+
 * long entries isn't practical.
 */
export function SearchableSelect({ value, options, placeholder, emptyLabel, onChange, allowFreeText }: SearchableSelectProps) {
  const [isOpen, setIsOpen] = useState(false)
  const [search, setSearch] = useState('')
  const containerRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!isOpen) return
    function handleClickOutside(e: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setIsOpen(false)
        setSearch('')
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [isOpen])

  const filtered = search
    ? options.filter((o) => o.text.toLowerCase().includes(search.toLowerCase()))
    : options

  const trimmedSearch = search.trim()
  const hasExactMatch = filtered.some((o) => o.text.toLowerCase() === trimmedSearch.toLowerCase())
  const showFreeTextOption = allowFreeText && trimmedSearch.length > 0 && !hasExactMatch

  function commitFreeText() {
    onChange(trimmedSearch)
    setIsOpen(false)
    setSearch('')
  }

  return (
    <div ref={containerRef} className="relative">
      <button
        type="button"
        onClick={() => setIsOpen((prev) => !prev)}
        className="mt-1 flex h-9 w-full items-center justify-between gap-2 rounded-lg border border-input bg-background px-2 text-left text-sm outline-none focus:ring-2 focus:ring-ring"
      >
        <span className={clsx('truncate', !value && 'text-muted-foreground')}>
          {value || (options.length ? placeholder : emptyLabel)}
        </span>
        <div className="flex shrink-0 items-center gap-1">
          {value && (
            <span
              role="button"
              tabIndex={0}
              onClick={(e) => { e.stopPropagation(); onChange(null) }}
              className="rounded p-0.5 text-muted-foreground hover:text-foreground"
            >
              <X className="size-3.5" />
            </span>
          )}
          <ChevronDown className="size-3.5 text-muted-foreground" />
        </div>
      </button>

      {isOpen && (
        <div className="absolute left-0 top-full z-50 mt-1 w-full overflow-hidden rounded-lg border border-border bg-card shadow-lg">
          {(allowFreeText || options.length > 5) && (
            <input
              autoFocus
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === 'Enter' && showFreeTextOption) {
                  e.preventDefault()
                  commitFreeText()
                }
              }}
              placeholder={allowFreeText ? 'Search or type your own...' : 'Search...'}
              className="w-full border-b border-border bg-background px-2 py-1.5 text-sm outline-none"
            />
          )}
          <div className="max-h-48 overflow-y-auto py-1">
            {showFreeTextOption && (
              <button
                type="button"
                onClick={commitFreeText}
                className="flex w-full items-start gap-2 whitespace-normal px-2 py-1.5 text-left text-sm text-primary hover:bg-muted"
              >
                Use &ldquo;{trimmedSearch}&rdquo;
              </button>
            )}
            {filtered.length === 0 && !showFreeTextOption && (
              <div className="px-2 py-1.5 text-sm text-muted-foreground">No matches.</div>
            )}
            {filtered.map((option) => (
              <button
                key={option.id}
                type="button"
                onClick={() => { onChange(option.text, option.id); setIsOpen(false); setSearch('') }}
                className="flex w-full items-start gap-2 whitespace-normal px-2 py-1.5 text-left text-sm hover:bg-muted"
              >
                <Check className={clsx('mt-0.5 size-3.5 shrink-0', option.text === value ? 'opacity-100 text-primary' : 'opacity-0')} />
                <span>{option.text}</span>
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}
