import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { X } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { SearchableSelect } from '@/components/ui/SearchableSelect'
import type { Team } from '@/api/teams'
import type { Group } from '@/api/groups'
import { usersApi, type CreateUserInput, type UserRole } from '@/api/users'

const roles: UserRole[] = [
  'Admin', 'Supervisor', 'TeamLead', 'QaEvaluator', 'Agent',
  'GroupAdmin', 'TeamAdmin', 'ReportsAnalyst', 'CalibrationAnalyst',
]

interface CreateUserFormProps {
  teams: Team[]
  groups: Group[]
  onSubmit: (input: CreateUserInput) => void
  onCancel: () => void
  isSubmitting: boolean
}

export function CreateUserForm({ teams, groups, onSubmit, onCancel, isSubmitting }: CreateUserFormProps) {
  const [displayName, setDisplayName] = useState('')
  const [email, setEmail] = useState('')
  const [manualEmail, setManualEmail] = useState(false)
  const [role, setRole] = useState<UserRole>('Agent')
  const [employmentType, setEmploymentType] = useState('')
  const [teamIds, setTeamIds] = useState<string[]>([])
  const [groupIds, setGroupIds] = useState<string[]>([])

  // Directory-backed picker (Entra ID group) so an Admin selects a real account instead of
  // free-typing an email — falls back to manual entry if Graph is unreachable/misconfigured or
  // the person just isn't in that group yet.
  const directoryQuery = useQuery({
    queryKey: ['users', 'directory-candidates'],
    queryFn: () => usersApi.getDirectoryCandidates(),
    retry: false,
  })
  const candidates = (directoryQuery.data ?? []).filter((c) => c.email)
  const showManualEmail = manualEmail || directoryQuery.isError
  const selectedCandidate = email ? candidates.find((c) => c.email === email) : undefined
  const selectedCandidateLabel = selectedCandidate ? `${selectedCandidate.displayName} <${selectedCandidate.email}>` : email || null

  const inputClass = 'w-full rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring'
  const ready = displayName && email

  function toggle(list: string[], setList: (v: string[]) => void, id: string) {
    setList(list.includes(id) ? list.filter((x) => x !== id) : [...list, id])
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!ready) return
    onSubmit({ displayName, email, role, employmentType: employmentType || null, teamIds, groupIds })
  }

  return (
    <Card>
      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <div className="grid gap-4 md:grid-cols-2">
          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">Name</span>
            <input className={inputClass} value={displayName} onChange={(e) => setDisplayName(e.target.value)} required />
          </label>
          <label className="flex flex-col gap-1.5 text-sm">
            <div className="flex items-center justify-between">
              <span className="font-medium">Email</span>
              {!directoryQuery.isError && (
                <button
                  type="button"
                  onClick={() => setManualEmail((v) => !v)}
                  className="text-xs text-primary hover:underline"
                >
                  {showManualEmail ? 'Pick from directory instead' : "Can't find them? Enter manually"}
                </button>
              )}
            </div>
            {showManualEmail ? (
              <input type="email" className={inputClass} value={email} onChange={(e) => setEmail(e.target.value)} required />
            ) : (
              <SearchableSelect
                value={selectedCandidateLabel}
                options={candidates.map((c) => ({ id: c.email!, text: `${c.displayName} <${c.email}>` }))}
                placeholder={directoryQuery.isLoading ? 'Loading directory...' : 'Select a person...'}
                emptyLabel="No matching accounts in the directory"
                onChange={(_, id) => {
                  setEmail(id ?? '')
                  const candidate = candidates.find((c) => c.email === id)
                  if (candidate && !displayName) setDisplayName(candidate.displayName)
                }}
              />
            )}
            {directoryQuery.isError && (
              <p className="text-xs text-status-fail">Directory lookup unavailable — enter the email manually.</p>
            )}
          </label>
          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">Role</span>
            <select className={inputClass} value={role} onChange={(e) => setRole(e.target.value as UserRole)}>
              {roles.map((r) => (
                <option key={r} value={r}>{r}</option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium">Employment type (optional)</span>
            <input className={inputClass} value={employmentType} onChange={(e) => setEmploymentType(e.target.value)} />
          </label>
        </div>

        <div className="flex flex-col gap-2 text-sm">
          <span className="font-medium">Groups</span>
          <div className="flex flex-wrap gap-2">
            {groups.map((g) => (
              <button
                type="button"
                key={g.id}
                onClick={() => toggle(groupIds, setGroupIds, g.id)}
                className={
                  'rounded-lg border px-3 py-1.5 text-xs font-medium transition-colors ' +
                  (groupIds.includes(g.id)
                    ? 'border-primary bg-primary/10 text-primary'
                    : 'border-border text-muted-foreground hover:border-primary/50 hover:text-foreground')
                }
              >
                {g.name}
              </button>
            ))}
          </div>
        </div>

        <div className="flex flex-col gap-2 text-sm">
          <span className="font-medium">Teams</span>
          <div className="flex max-h-32 flex-wrap gap-2 overflow-y-auto">
            {teams.map((t) => (
              <button
                type="button"
                key={t.id}
                onClick={() => toggle(teamIds, setTeamIds, t.id)}
                className={
                  'rounded-lg border px-3 py-1.5 text-xs font-medium transition-colors ' +
                  (teamIds.includes(t.id)
                    ? 'border-primary bg-primary/10 text-primary'
                    : 'border-border text-muted-foreground hover:border-primary/50 hover:text-foreground')
                }
              >
                {t.name}
              </button>
            ))}
          </div>
        </div>

        <div className="flex justify-end gap-2">
          <Button type="button" variant="secondary" onClick={onCancel}>
            <X className="size-4" /> Cancel
          </Button>
          <Button type="submit" disabled={!ready || isSubmitting}>
            {isSubmitting ? 'Creating...' : 'Create user'}
          </Button>
        </div>
      </form>
    </Card>
  )
}
