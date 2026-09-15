import { apiRequest } from './client'

export interface ScorecardCategory {
  id: string
  name: string
}

export const scorecardCategoriesApi = {
  list: () => apiRequest<ScorecardCategory[]>('/api/v1/scorecard-categories'),

  create: (name: string) =>
    apiRequest<{ id: string }>('/api/v1/scorecard-categories', { method: 'POST', body: JSON.stringify({ name }) }),

  update: (id: string, name: string) =>
    apiRequest<void>(`/api/v1/scorecard-categories/${id}`, { method: 'PUT', body: JSON.stringify({ name }) }),

  delete: (id: string) => apiRequest<void>(`/api/v1/scorecard-categories/${id}`, { method: 'DELETE' }),
}
