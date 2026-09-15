import { apiRequest } from './client'

export type EvaluationStatus = 'Draft' | 'Submitted' | 'Acknowledged' | 'Disputed' | 'Resolved'

export interface EvaluationSummary {
  id: string
  scorecardName: string
  agentId: string
  agentName: string
  evaluatorId: string
  evaluatorName: string
  reference: string | null
  status: EvaluationStatus
  totalScore: number | null
  submittedAt: string | null
}

export interface AnswerOption {
  label: string
  value: number
  isFailSection: boolean
  isFailAll: boolean
  isNotApplicable: boolean
}

export interface EvaluationAnswer {
  questionId: string
  questionText: string
  sectionName: string
  weight: number
  answerOptions: AnswerOption[]
  answerValue: string | null
  causeCode: string | null
  comment: string | null
  score: number | null
}

export interface EvaluationDetail {
  id: string
  scorecardId: string
  scorecardName: string
  scorecardTargetPercentage: number | null
  agentId: string
  agentName: string
  evaluatorId: string
  evaluatorName: string
  reference: string | null
  status: EvaluationStatus
  eventOccurredAt: string | null
  eventDurationSeconds: number | null
  evaluatorNotes: string | null
  disputeReason: string | null
  resolutionNotes: string | null
  submittedAt: string | null
  totalScore: number | null
  answers: EvaluationAnswer[]
}

export interface CreateEvaluationInput {
  scorecardId: string
  agentId: string
  evaluatorId: string
  eventTypeId?: string | null
  eventSubTypeId?: string | null
  reference?: string | null
  eventOccurredAt?: string | null
  eventDurationSeconds?: number | null
}

// No score field — score is derived server-side from the chosen answer option's
// configured value, never sent by the client.
export interface AnswerInput {
  questionId: string
  answerValue: string | null
  causeCode: string | null
  comment: string | null
}

export interface EvaluationQueueFilters {
  agentId?: string
  evaluatorId?: string
  teamId?: string
  status?: EvaluationStatus
  scorecardId?: string
  page?: number
  pageSize?: number
}

function toQueryString(filters: EvaluationQueueFilters): string {
  const params = new URLSearchParams()
  for (const [key, value] of Object.entries(filters)) {
    if (value !== undefined && value !== null) params.set(key, String(value))
  }
  const qs = params.toString()
  return qs ? `?${qs}` : ''
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export const evaluationsApi = {
  getQueue: (filters: EvaluationQueueFilters = {}) =>
    apiRequest<PagedResult<EvaluationSummary>>(`/api/v1/evaluations${toQueryString(filters)}`),

  getById: (id: string) => apiRequest<EvaluationDetail>(`/api/v1/evaluations/${id}`),

  create: (input: CreateEvaluationInput) =>
    apiRequest<{ id: string }>('/api/v1/evaluations', { method: 'POST', body: JSON.stringify(input) }),

  saveDraftAnswers: (id: string, answers: AnswerInput[]) =>
    apiRequest<void>(`/api/v1/evaluations/${id}/answers`, { method: 'PUT', body: JSON.stringify({ answers }) }),

  submit: (id: string, evaluatorNotes: string | null) =>
    apiRequest<void>(`/api/v1/evaluations/${id}/submit`, { method: 'POST', body: JSON.stringify({ evaluatorNotes }) }),

  acknowledge: (id: string) =>
    apiRequest<void>(`/api/v1/evaluations/${id}/acknowledge`, { method: 'POST' }),

  dispute: (id: string, disputeReason: string) =>
    apiRequest<void>(`/api/v1/evaluations/${id}/dispute`, { method: 'POST', body: JSON.stringify({ disputeReason }) }),

  resolveDispute: (id: string, resolutionNotes: string) =>
    apiRequest<void>(`/api/v1/evaluations/${id}/resolve-dispute`, { method: 'POST', body: JSON.stringify({ resolutionNotes }) }),
}
