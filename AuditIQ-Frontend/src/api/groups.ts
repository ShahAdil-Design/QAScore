import { apiRequest } from './client'

export interface Group {
  id: string
  name: string
}

export const groupsApi = {
  list: () => apiRequest<Group[]>('/api/v1/groups'),
}
