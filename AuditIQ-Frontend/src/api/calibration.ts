import { apiRequest } from './client'
import type { AnswerOption, PagedResult } from './evaluations'

export interface CalibrationListSummary {
  id: string
  name: string
  visibilityScope: string
  createdByName: string
  createdAt: string
  groupNames: string[]
  teamNames: string[]
  itemCount: number
  ratedItemCount: number
}

export interface CalibrationCandidateEvaluation {
  id: string
  reference: string | null
  agentId: string
  agentName: string
  evaluatorId: string
  evaluatorName: string
  eventOccurredAt: string | null
  submittedAt: string | null
  teamNames: string[]
  groupNames: string[]
  scorecardName: string
  categoryName: string
  eventTypeName: string | null
  eventSubTypeName: string | null
  totalScore: number | null
}

export interface CalibrationListItemSummary {
  id: string
  evaluationId: string
  agentId: string
  agentName: string
  evaluatorName: string
  eventOccurredAt: string | null
  teamNames: string[]
  scorecardName: string
  reference: string | null
  originalScore: number | null
  myScore: number | null
  ratingCount: number
}

export interface CalibrationAnswer {
  questionId: string
  answerValue: string | null
  causeCode: string | null
  comment: string | null
  score: number | null
}

// One scorecard question plus the ORIGINAL evaluator's answer to it — what a
// calibrator re-scores against and compares their own rating to.
export interface CalibrationQuestion {
  questionId: string
  questionText: string
  sectionName: string
  weight: number
  answerOptions: AnswerOption[]
  originalAnswerValue: string | null
  originalCauseCode: string | null
  originalComment: string | null
  originalScore: number | null
}

export interface CalibrationRating {
  evaluatorId: string
  evaluatorName: string
  score: number | null
  answers: CalibrationAnswer[]
}

export interface CalibrationItemDetail {
  id: string
  calibrationListId: string
  evaluationId: string
  scorecardName: string
  agentId: string
  agentName: string
  evaluatorId: string
  evaluatorName: string
  teamNames: string[]
  eventTypeName: string | null
  eventSubTypeName: string | null
  eventOccurredAt: string | null
  eventDurationSeconds: number | null
  reference: string | null
  originalScore: number | null
  questions: CalibrationQuestion[]
  ratings: CalibrationRating[]
  variance: number | null
}

export interface CreateCalibrationListInput {
  name: string
  visibilityScope: string
  createdByUserId: string
}

export interface CalibrationCandidateFilters {
  dateFrom?: string
  dateTo?: string
  groupId?: string
  teamId?: string
  evaluatorId?: string
  reference?: string
  scorecardId?: string
  categoryId?: string
  scoreMin?: number
  scoreMax?: number
  random?: boolean
  sampleSize?: number
  page?: number
  pageSize?: number
}

// No score field — score is derived server-side from the chosen answer option's
// configured value, never sent by the client.
export interface CalibrationAnswerInput {
  questionId: string
  answerValue: string | null
  causeCode: string | null
  comment: string | null
}

function toQueryString(params: object): string {
  const qs = new URLSearchParams()
  for (const [key, value] of Object.entries(params) as [string, unknown][]) {
    if (value !== undefined && value !== null && value !== '') qs.set(key, String(value))
  }
  const s = qs.toString()
  return s ? `?${s}` : ''
}

export const calibrationApi = {
  getLists: (search?: string, page = 1, pageSize = 25) =>
    apiRequest<PagedResult<CalibrationListSummary>>(`/api/v1/calibration-lists${toQueryString({ search, page, pageSize })}`),

  create: (input: CreateCalibrationListInput) =>
    apiRequest<{ id: string }>('/api/v1/calibration-lists', { method: 'POST', body: JSON.stringify(input) }),

  getCandidates: (filters: CalibrationCandidateFilters = {}) =>
    apiRequest<PagedResult<CalibrationCandidateEvaluation>>(`/api/v1/calibration-lists/candidates${toQueryString(filters)}`),

  addItems: (listId: string, evaluationIds: string[]) =>
    apiRequest<void>(`/api/v1/calibration-lists/${listId}/items`, {
      method: 'POST',
      body: JSON.stringify({ evaluationIds }),
    }),

  getItems: (listId: string, evaluatorId?: string, page = 1, pageSize = 50) =>
    apiRequest<PagedResult<CalibrationListItemSummary>>(
      `/api/v1/calibration-lists/${listId}/items${toQueryString({ evaluatorId, page, pageSize })}`,
    ),

  getItemDetail: (itemId: string) =>
    apiRequest<CalibrationItemDetail>(`/api/v1/calibration-lists/items/${itemId}`),

  saveAnswers: (itemId: string, evaluatorId: string, answers: CalibrationAnswerInput[]) =>
    apiRequest<void>(`/api/v1/calibration-lists/items/${itemId}/answers`, {
      method: 'PUT',
      body: JSON.stringify({ evaluatorId, answers }),
    }),
}
