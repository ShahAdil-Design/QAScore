import { apiRequest } from './client'

export interface Team {
  id: string
  name: string
  groupId: string
  groupName: string
}

export const teamsApi = {
  list: (groupId?: string) => apiRequest<Team[]>(`/api/v1/teams${groupId ? `?groupId=${groupId}` : ''}`),
}
