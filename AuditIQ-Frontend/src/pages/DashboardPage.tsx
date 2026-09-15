import { useState } from 'react'
import { Activity, ClipboardCheck } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import clsx from 'clsx'
import { SectionHeader } from '@/components/ui/SectionHeader'
import { StatCard } from '@/components/ui/StatCard'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { dashboardApi } from '@/api/dashboard'
import { scorecardsApi } from '@/api/scorecards'
import { usersApi } from '@/api/users'
import { EmployeeDashboardView } from '@/features/dashboard/EmployeeDashboardView'

type ViewMode = 'team' | 'agents' | 'employee'

export function DashboardPage() {
  const [view, setView] = useState<ViewMode>('team')
  // Set by picking a row on the Agents tab — jumps straight to that agent's Employee view
  // rather than making you find them again in that tab's own picker.
  const [selectedAgentId, setSelectedAgentId] = useState<string | null>(null)

  function viewAgent(agentId: string) {
    setSelectedAgentId(agentId)
    setView('employee')
  }

  return (
    <>
      <div className="mb-4 inline-flex rounded-lg border border-border bg-card p-1">
        {([
          ['team', 'Overview'],
          ['agents', 'Agents'],
          ['employee', 'Employee view'],
        ] as const).map(([mode, label]) => (
          <button
            key={mode}
            onClick={() => setView(mode)}
            className={clsx(
              'rounded-md px-3 py-1.5 text-sm font-medium transition-colors',
              view === mode ? 'bg-primary text-primary-foreground' : 'text-muted-foreground hover:text-foreground',
            )}
          >
            {label}
          </button>
        ))}
      </div>

      {view === 'team' && <TeamOverviewView />}
      {view === 'agents' && <AgentsView onSelectAgent={viewAgent} />}
      {view === 'employee' && <EmployeeDashboardView key={selectedAgentId ?? 'default'} initialAgentId={selectedAgentId ?? undefined} />}
    </>
  )
}

function TeamOverviewView() {
  const navigate = useNavigate()
  const [scorecardId, setScorecardId] = useState('')
  const [supervisorId, setSupervisorId] = useState('')

  const scorecardsQuery = useQuery({ queryKey: ['scorecards'], queryFn: () => scorecardsApi.list() })
  const supervisorsQuery = useQuery({ queryKey: ['users', 'Supervisor'], queryFn: () => usersApi.list('Supervisor') })
  const summaryQuery = useQuery({
    queryKey: ['dashboard-summary', scorecardId, supervisorId],
    queryFn: () => dashboardApi.getSummary(scorecardId || undefined, supervisorId || undefined),
  })

  if (summaryQuery.isLoading) return <p className="text-sm text-muted-foreground">Loading dashboard...</p>
  if (summaryQuery.isError) {
    return <p className="text-sm text-status-fail">Failed to load dashboard: {(summaryQuery.error as Error).message}</p>
  }

  const summary = summaryQuery.data!
  const trendValues = summary.weeklyTrend.map((w) => w.averageScore ?? 0)
  const maxTrendValue = Math.max(...trendValues, 1)

  const stats = [
    { label: 'Average QA score', value: summary.averageScore !== null ? `${summary.averageScore.toFixed(2)}%` : '—' },
    { label: 'Evaluations completed', value: String(summary.evaluationsCompleted) },
    { label: 'Fails', value: String(summary.failsCount) },
    { label: 'Disputed scores', value: String(summary.disputedCount) },
    { label: 'Agent coverage', value: `${summary.agentCoveragePercent}%` },
  ]

  const coverage = [
    { label: 'Agent coverage', value: summary.agentCoveragePercent },
    { label: 'Scorecard adoption', value: summary.scorecardAdoptionPercent },
    { label: 'Calibration completion', value: summary.calibrationCompletionPercent },
  ]

  const selectClass = 'rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring'

  return (
    <>
      <SectionHeader
        title="Overview"
        description="Role-based summary of your quality evaluation program (Section 3 — role-based dashboards)."
        action={
          <div className="flex gap-2">
            <Button variant="secondary" onClick={() => navigate('/calibration')}>
              <Activity className="size-4" />
              View calibrations
            </Button>
            <Button onClick={() => navigate('/score')}>
              <ClipboardCheck className="size-4" />
              Score an interaction
            </Button>
          </div>
        }
      />

      <div className="mb-4 flex flex-wrap gap-3">
        <select className={selectClass} value={scorecardId} onChange={(e) => setScorecardId(e.target.value)}>
          <option value="">All Scorecards</option>
          {(scorecardsQuery.data ?? []).map((s) => (
            <option key={s.id} value={s.id}>{s.name}</option>
          ))}
        </select>
        <select className={selectClass} value={supervisorId} onChange={(e) => setSupervisorId(e.target.value)}>
          <option value="">All Supervisors</option>
          {(supervisorsQuery.data ?? []).map((u) => (
            <option key={u.id} value={u.id}>{u.displayName}</option>
          ))}
        </select>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-5">
        {stats.map((s) => (
          <StatCard key={s.label} icon={Activity} label={s.label} value={s.value} />
        ))}
      </div>

      <div className="mt-6 grid gap-6 lg:grid-cols-2">
        <Card>
          <h3 className="font-semibold">Quality score trend</h3>
          <p className="mt-1 text-sm text-muted-foreground">Average evaluation score over time (weekly, last 12 weeks)</p>
          <div className="mt-8 flex h-44 items-end gap-2 border-b border-border">
            {summary.weeklyTrend.map((w, i) => (
              <div
                key={i}
                title={w.averageScore !== null ? `${w.weekStart}: ${w.averageScore.toFixed(2)}%` : `${w.weekStart}: no evaluations`}
                className="flex-1 rounded-t bg-primary/80 hover:bg-primary"
                style={{ height: `${w.averageScore !== null ? Math.max((w.averageScore / maxTrendValue) * 100, 2) : 0}%` }}
              />
            ))}
          </div>
          <div className="mt-3 flex justify-between text-xs text-muted-foreground">
            <span>12 weeks ago</span>
            <span>6 weeks ago</span>
            <span>Today</span>
          </div>
        </Card>

        <Card>
          <h3 className="font-semibold">Program health</h3>
          <p className="mt-1 text-sm text-muted-foreground">Coverage across your quality operations</p>
          <div className="mt-6 flex flex-col gap-5">
            {coverage.map(({ label, value }) => (
              <div key={label}>
                <div className="mb-2 flex justify-between text-sm">
                  <span>{label}</span>
                  <span className="font-semibold">{value}%</span>
                </div>
                <div className="h-2 rounded-full bg-muted">
                  <div className="h-2 rounded-full bg-primary" style={{ width: `${value}%` }} />
                </div>
              </div>
            ))}
          </div>
        </Card>
      </div>
    </>
  )
}

function AgentsView({ onSelectAgent }: { onSelectAgent: (agentId: string) => void }) {
  const [supervisorId, setSupervisorId] = useState('')

  const supervisorsQuery = useQuery({ queryKey: ['users', 'Supervisor'], queryFn: () => usersApi.list('Supervisor') })
  const agentsQuery = useQuery({
    queryKey: ['dashboard-agents-overview', supervisorId],
    queryFn: () => dashboardApi.getAgentsOverview(supervisorId || undefined),
  })

  const selectClass = 'rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring'

  return (
    <>
      <SectionHeader title="Agents" description="Every agent's aggregate score, evaluation count, disputes, and fails." />

      <div className="mb-4">
        <select className={selectClass} value={supervisorId} onChange={(e) => setSupervisorId(e.target.value)}>
          <option value="">All Supervisors</option>
          {(supervisorsQuery.data ?? []).map((u) => (
            <option key={u.id} value={u.id}>{u.displayName}</option>
          ))}
        </select>
      </div>

      <Card>
        {agentsQuery.isLoading && <p className="text-sm text-muted-foreground">Loading agents...</p>}
        {agentsQuery.isError && (
          <p className="text-sm text-status-fail">Failed to load agents: {(agentsQuery.error as Error).message}</p>
        )}
        {agentsQuery.isSuccess && agentsQuery.data.length === 0 && (
          <p className="text-sm text-muted-foreground">No agents found.</p>
        )}
        {agentsQuery.isSuccess && agentsQuery.data.length > 0 && (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[560px] text-left text-sm">
              <thead className="text-xs text-muted-foreground">
                <tr>
                  <th className="pb-2 pr-2 font-medium">Agent</th>
                  <th className="pb-2 px-2 font-medium">Avg score</th>
                  <th className="pb-2 px-2 font-medium">Evaluations</th>
                  <th className="pb-2 px-2 font-medium">Disputed</th>
                  <th className="pb-2 px-2 font-medium">Fails</th>
                </tr>
              </thead>
              <tbody>
                {agentsQuery.data.map((a) => (
                  <tr
                    key={a.agentId}
                    onClick={() => onSelectAgent(a.agentId)}
                    className="cursor-pointer border-t border-border hover:bg-muted"
                  >
                    <td className="py-2 pr-2 font-medium">{a.displayName}</td>
                    <td className="py-2 px-2">{a.averageScore !== null ? `${a.averageScore.toFixed(2)}%` : '—'}</td>
                    <td className="py-2 px-2">{a.evaluationCount}</td>
                    <td className="py-2 px-2">{a.disputedCount}</td>
                    <td className="py-2 px-2">{a.failsCount}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>
    </>
  )
}
