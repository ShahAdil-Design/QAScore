import { apiRequest } from './client'

export interface WeeklyScorePoint {
  weekStart: string
  averageScore: number | null
}

export interface DashboardSummary {
  averageScore: number | null
  evaluationsCompleted: number
  disputedCount: number
  failsCount: number
  agentCoveragePercent: number
  scorecardAdoptionPercent: number
  calibrationCompletionPercent: number
  weeklyTrend: WeeklyScorePoint[]
}

export interface AgentOverviewRow {
  agentId: string
  displayName: string
  averageScore: number | null
  evaluationCount: number
  disputedCount: number
  failsCount: number
}

export interface AgentWeeklyScorePoint {
  weekStart: string
  myScore: number | null
  teamScore: number | null
}

export interface RecentScore {
  evaluationId: string
  scorecardName: string
  status: string
  submittedAt: string | null
  totalScore: number | null
}

export interface AgentDashboard {
  agentName: string
  teamName: string | null
  myOverallScore: number | null
  teamOverallScore: number | null
  kudosCount: number
  // Always 0 for now — there's no "flagged evaluation" concept in the backend yet.
  flagCount: number
  weeklyTrend: AgentWeeklyScorePoint[]
  recentScores: RecentScore[]
}

export const dashboardApi = {
  getSummary: (scorecardId?: string, supervisorId?: string) => {
    const params = new URLSearchParams()
    if (scorecardId) params.set('scorecardId', scorecardId)
    if (supervisorId) params.set('supervisorId', supervisorId)
    const qs = params.toString()
    return apiRequest<DashboardSummary>(`/api/v1/dashboard/summary${qs ? `?${qs}` : ''}`)
  },

  getAgentsOverview: (supervisorId?: string) =>
    apiRequest<AgentOverviewRow[]>(`/api/v1/dashboard/agents-overview${supervisorId ? `?supervisorId=${supervisorId}` : ''}`),

  getAgentDashboard: (agentId: string, days = 30) =>
    apiRequest<AgentDashboard>(`/api/v1/dashboard/agents/${agentId}?days=${days}`),
}
