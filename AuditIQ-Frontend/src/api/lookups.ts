import { apiRequest } from './client'

export interface LookupItem {
  id: string
  text: string
}

export interface EventSubType {
  id: string
  name: string
}

export interface EventType {
  id: string
  name: string
  subTypes: EventSubType[]
}

export const lookupsApi = {
  getCauseCodes: () => apiRequest<LookupItem[]>('/api/v1/lookups/cause-codes'),
  getComments: () => apiRequest<LookupItem[]>('/api/v1/lookups/comments'),
  getEventTypes: (scorecardId: string) =>
    apiRequest<EventType[]>(`/api/v1/lookups/event-types?scorecardId=${scorecardId}`),

  createCauseCode: (text: string) =>
    apiRequest<{ id: string }>('/api/v1/lookups/cause-codes', { method: 'POST', body: JSON.stringify({ text }) }),
  updateCauseCode: (id: string, text: string) =>
    apiRequest<void>(`/api/v1/lookups/cause-codes/${id}`, { method: 'PUT', body: JSON.stringify({ text }) }),
  deleteCauseCode: (id: string) => apiRequest<void>(`/api/v1/lookups/cause-codes/${id}`, { method: 'DELETE' }),

  createComment: (text: string) =>
    apiRequest<{ id: string }>('/api/v1/lookups/comments', { method: 'POST', body: JSON.stringify({ text }) }),
  updateComment: (id: string, text: string) =>
    apiRequest<void>(`/api/v1/lookups/comments/${id}`, { method: 'PUT', body: JSON.stringify({ text }) }),
  deleteComment: (id: string) => apiRequest<void>(`/api/v1/lookups/comments/${id}`, { method: 'DELETE' }),
}
