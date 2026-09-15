import { apiRequest } from './client'

export interface ScorePoint {
  date: string
  score: number
}

export interface AgentPerformanceSummary {
  agentId: string
  agentName: string
  averageScore: number | null
  evaluationCount: number
  disputedCount: number
  trend: ScorePoint[]
}

export interface AgentResult {
  agentId: string
  agentName: string
  averageScore: number | null
  evaluationCount: number
}

export interface TeamResults {
  teamId: string
  teamName: string
  agents: AgentResult[]
}

export interface Kudos {
  id: string
  fromUserName: string
  toUserName: string
  message: string
  createdAt: string
}

export const reviewsApi = {
  getAgentPerformance: (agentId: string) =>
    apiRequest<AgentPerformanceSummary>(`/api/v1/reviews/agents/${agentId}/performance`),

  getTeamResults: (teamId: string) => apiRequest<TeamResults>(`/api/v1/reviews/teams/${teamId}/results`),

  getKudos: (userId: string) => apiRequest<Kudos[]>(`/api/v1/reviews/kudos/${userId}`),

  giveKudos: (fromUserId: string, toUserId: string, message: string) =>
    apiRequest<{ id: string }>('/api/v1/reviews/kudos', {
      method: 'POST',
      body: JSON.stringify({ fromUserId, toUserId, message }),
    }),
}
