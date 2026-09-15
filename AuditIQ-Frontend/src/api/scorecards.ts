import { apiRequest } from './client'
import type { AnswerOption } from './evaluations'

export interface ScorecardSummary {
  id: string
  name: string
  categoryName: string
  location: string | null
  targetPercentage: number | null
  maxScore: number | null
  version: number
  isLocked: boolean
  questionCount: number
  groupNames: string[]
}

export interface ScorecardQuestion {
  id: string
  sectionName: string
  text: string
  weight: number
  isFailLogic: boolean
  sortOrder: number
  answerOptions: AnswerOption[]
  maxScore: number
}

export interface ScorecardDetail {
  id: string
  name: string
  description: string | null
  scorecardType: string
  categoryName: string
  location: string | null
  targetPercentage: number | null
  maxScore: number | null
  version: number
  isLocked: boolean
  isArchived: boolean
  groupIds: string[]
  groupNames: string[]
  questions: ScorecardQuestion[]
  tipSheetTitles: string[]
}

export interface AnswerOptionInput {
  label: string
  value: number
  isFailSection: boolean
  isFailAll: boolean
  isNotApplicable: boolean
}

export interface ScorecardQuestionInput {
  sectionName: string
  text: string
  weight: number
  isFailLogic: boolean
  sortOrder: number
  answerOptions: AnswerOptionInput[]
}

export interface ScorecardWriteInput {
  name: string
  description: string | null
  scorecardType: string
  categoryId: string
  location: string | null
  targetPercentage: number | null
  maxScore: number
  groupIds: string[]
  questions: ScorecardQuestionInput[]
}

export const scorecardsApi = {
  list: (categoryId?: string) =>
    apiRequest<ScorecardSummary[]>(`/api/v1/scorecards${categoryId ? `?categoryId=${categoryId}` : ''}`),

  getById: (id: string) => apiRequest<ScorecardDetail>(`/api/v1/scorecards/${id}`),

  create: (input: ScorecardWriteInput) =>
    apiRequest<{ id: string }>('/api/v1/scorecards', { method: 'POST', body: JSON.stringify(input) }),

  update: (id: string, input: ScorecardWriteInput) =>
    apiRequest<{ id: string }>(`/api/v1/scorecards/${id}`, { method: 'PUT', body: JSON.stringify(input) }),

  lock: (id: string) => apiRequest<void>(`/api/v1/scorecards/${id}/lock`, { method: 'POST' }),

  unlock: (id: string) => apiRequest<void>(`/api/v1/scorecards/${id}/unlock`, { method: 'POST' }),

  archive: (id: string) => apiRequest<void>(`/api/v1/scorecards/${id}`, { method: 'DELETE' }),
}
