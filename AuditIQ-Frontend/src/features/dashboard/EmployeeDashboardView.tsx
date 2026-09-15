import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Activity, Bell, ClipboardCheck, Sparkles, Users } from 'lucide-react'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { usersApi } from '@/api/users'
import { dashboardApi } from '@/api/dashboard'
import { useCurrentUser } from '@/auth/currentUserStore'

const periods = [
  { label: 'Last 7 days', days: 7 },
  { label: 'Last 30 days', days: 30 },
  { label: 'Last 90 days', days: 90 },
]

interface EmployeeDashboardViewProps {
  // Set when arriving from the Agents tab's "click a row" flow — takes priority over the
  // self/alphabetically-first default below. The parent remounts this component (via `key`)
  // whenever it changes, so this only needs to matter on initial mount.
  initialAgentId?: string
}

export function EmployeeDashboardView({ initialAgentId }: EmployeeDashboardViewProps = {}) {
  const navigate = useNavigate()
  const [agentId, setAgentId] = useState(initialAgentId ?? '')
  const [days, setDays] = useState(30)
  const me = useCurrentUser((s) => s.me)

  // Not restricted to Role=Agent — a Supervisor being evaluated by another Supervisor needs to
  // see their own "Employee view" too, same as any Agent would.
  const agentsQuery = useQuery({ queryKey: ['users', 'evaluee-eligible'], queryFn: () => usersApi.list() })
  // Default the view to yourself (whatever your role) instead of whoever's alphabetically
  // first — the picker is still there to browse anyone else.
  const defaultAgentId = (me?.id ?? agentsQuery.data?.[0]?.id) ?? ''
  const effectiveAgentId = agentId || defaultAgentId

  const dashboardQuery = useQuery({
    queryKey: ['agent-dashboard', effectiveAgentId, days],
    queryFn: () => dashboardApi.getAgentDashboard(effectiveAgentId, days),
    enabled: !!effectiveAgentId,
  })

  if (agentsQuery.isSuccess && agentsQuery.data.length === 0) {
    return <Card className="text-sm text-muted-foreground">No agents exist yet.</Card>
  }

  const data = dashboardQuery.data
  const firstName = data?.agentName.split(' ')[0] ?? '...'
  const maxTrendValue = Math.max(
    ...(data?.weeklyTrend.flatMap((w) => [w.myScore ?? 0, w.teamScore ?? 0]) ?? [1]),
    1,
  )

  return (
    <>
      <div className="mb-2 flex items-center justify-between">
        <p className="text-sm font-medium text-primary">Employee view</p>
        <select
          className="rounded-lg border border-input bg-background px-2 py-1 text-xs outline-none focus:ring-2 focus:ring-ring"
          value={effectiveAgentId}
          onChange={(e) => setAgentId(e.target.value)}
        >
          {agentsQuery.data?.map((u) => (
            <option key={u.id} value={u.id}>{u.displayName}</option>
          ))}
        </select>
      </div>

      <div className="mb-6 flex flex-col justify-between gap-4 md:flex-row md:items-end">
        <div>
          <h2 className="text-2xl font-semibold tracking-tight">Welcome back, {firstName}</h2>
          <p className="mt-2 text-sm text-muted-foreground">Your quality performance and recent feedback in one place.</p>
        </div>
        <div className="flex gap-2">
          <Button variant="secondary" onClick={() => navigate('/calibration')}>
            <Activity className="size-4" /> Calibration
          </Button>
          <Button onClick={() => navigate('/score')}>
            <ClipboardCheck className="size-4" /> View scorecard
          </Button>
        </div>
      </div>

      {!data ? (
        <p className="text-sm text-muted-foreground">Loading...</p>
      ) : (
        <>
          <Card className="mb-6 flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
            <div className="flex items-center gap-3">
              <div className="flex size-11 items-center justify-center rounded-full bg-primary/10 font-semibold text-primary">
                {firstName[0]}
              </div>
              <div>
                <p className="font-semibold">{data.agentName}</p>
                <p className="text-sm text-muted-foreground">{data.teamName ?? 'No team assigned'} · Team member</p>
              </div>
            </div>
            <select
              className="rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring"
              value={days}
              onChange={(e) => setDays(Number(e.target.value))}
            >
              {periods.map((p) => (
                <option key={p.days} value={p.days}>{p.label}</option>
              ))}
            </select>
          </Card>

          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
            <Card className="bg-status-pass text-white">
              <div className="flex items-center justify-between text-sm opacity-90">
                Team overall score
                <Users className="size-4" />
              </div>
              <div className="mt-4 text-2xl font-semibold">
                {data.teamOverallScore !== null ? `${data.teamOverallScore}%` : '—'}
              </div>
            </Card>
            <Card>
              <div className="flex items-center justify-between text-sm text-muted-foreground">
                My overall score
                <Activity className="size-4 text-primary" />
              </div>
              <div className="mt-4 text-2xl font-semibold">
                {data.myOverallScore !== null ? `${data.myOverallScore}%` : '—'}
              </div>
            </Card>
            <Card className="bg-status-warn text-white">
              <div className="flex items-center justify-between text-sm opacity-90">
                Kudos
                <Sparkles className="size-4" />
              </div>
              <div className="mt-4 text-2xl font-semibold">{data.kudosCount}</div>
            </Card>
            <Card className="bg-orange-500 text-white" title="No 'flagged evaluation' concept exists yet — always 0 for now">
              <div className="flex items-center justify-between text-sm opacity-90">
                Flags
                <Bell className="size-4" />
              </div>
              <div className="mt-4 text-2xl font-semibold">{data.flagCount}</div>
            </Card>
          </div>

          <div className="mt-6 grid gap-6 lg:grid-cols-[1fr_360px]">
            <Card>
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="font-semibold">Quality score</h3>
                  <p className="mt-1 text-sm text-muted-foreground">Team versus personal performance by week</p>
                </div>
                <div className="flex items-center gap-3 text-xs text-muted-foreground">
                  <span className="flex items-center gap-1"><span className="size-2 rounded-full bg-status-pass" /> Team</span>
                  <span className="flex items-center gap-1"><span className="size-2 rounded-full bg-primary" /> Mine</span>
                </div>
              </div>
              <div className="mt-8 flex h-44 items-end gap-3 border-b border-border">
                {data.weeklyTrend.map((w, i) => (
                  <div key={i} className="flex h-full flex-1 items-end justify-center gap-1">
                    <div
                      title={w.teamScore !== null ? `Team: ${w.teamScore}%` : 'Team: no data'}
                      className="w-2.5 rounded-t bg-status-pass/80"
                      style={{ height: `${w.teamScore !== null ? Math.max((w.teamScore / maxTrendValue) * 100, 2) : 0}%` }}
                    />
                    <div
                      title={w.myScore !== null ? `Mine: ${w.myScore}%` : 'Mine: no data'}
                      className="w-2.5 rounded-t bg-primary/80"
                      style={{ height: `${w.myScore !== null ? Math.max((w.myScore / maxTrendValue) * 100, 2) : 0}%` }}
                    />
                  </div>
                ))}
              </div>
              <div className="mt-3 flex justify-between text-xs text-muted-foreground">
                {data.weeklyTrend.map((w, i) => (
                  <span key={i}>Week {new Date(w.weekStart).getDate()}</span>
                ))}
              </div>
            </Card>

            <Card>
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="font-semibold">Recent scores</h3>
                  <p className="mt-1 text-sm text-muted-foreground">Latest scorecard activity</p>
                </div>
                <button onClick={() => navigate('/review')} className="text-sm text-primary hover:underline">
                  All scores
                </button>
              </div>
              <div className="mt-4 flex flex-col gap-4">
                {data.recentScores.length === 0 && <p className="text-sm text-muted-foreground">No scores yet.</p>}
                {data.recentScores.map((s) => (
                  <div key={s.evaluationId} className="flex items-center justify-between border-b border-border pb-3 last:border-0">
                    <div>
                      <p className="text-sm font-medium">{s.scorecardName}</p>
                      <p className="text-xs text-muted-foreground">{s.status}</p>
                    </div>
                    <span className="text-sm font-semibold text-primary">
                      {s.totalScore !== null ? `${s.totalScore}%` : '—'}
                    </span>
                  </div>
                ))}
              </div>
            </Card>
          </div>
        </>
      )}
    </>
  )
}
