import { apiRequest } from './client'

// GroupAdmin/TeamAdmin/ReportsAnalyst/CalibrationAnalyst are Scorebuddy "Manage Users"
// admin-panel roles (see AuditIQ.Domain.Enums.UserRole) — dormant by design, granted no
// policy/screen access yet until deliberately configured in the Permissions page.
export type UserRole =
  | 'Admin' | 'Supervisor' | 'TeamLead' | 'QaEvaluator' | 'Agent'
  | 'GroupAdmin' | 'TeamAdmin' | 'ReportsAnalyst' | 'CalibrationAnalyst'

export interface UserSummary {
  id: string
  displayName: string
  email: string
  role: UserRole
  isActive: boolean
}

export interface TeamMembership {
  teamId: string
  teamName: string
  groupId: string
  groupName: string
}

export interface GroupMembership {
  groupId: string
  groupName: string
}

export interface UserDetail {
  id: string
  displayName: string
  email: string
  role: UserRole
  isActive: boolean
  employmentType: string | null
  notes: string | null
  teams: TeamMembership[]
  groups: GroupMembership[]
}

export interface CreateUserInput {
  displayName: string
  email: string
  role: UserRole
  employmentType: string | null
  teamIds: string[]
  groupIds: string[]
}

export interface DirectoryCandidate {
  objectId: string
  displayName: string
  email: string | null
}

export interface UpdateUserInput {
  displayName: string
  email: string
  role: UserRole
  employmentType: string | null
  notes: string | null
}

export const usersApi = {
  list: (role?: UserRole, includeInactive = false, groupId?: string) => {
    const params = new URLSearchParams()
    if (role) params.set('role', role)
    if (includeInactive) params.set('includeInactive', 'true')
    if (groupId) params.set('groupId', groupId)
    const qs = params.toString()
    return apiRequest<UserSummary[]>(`/api/v1/users${qs ? `?${qs}` : ''}`)
  },

  getById: (id: string) => apiRequest<UserDetail>(`/api/v1/users/${id}`),

  getMe: () => apiRequest<UserDetail>('/api/v1/users/me'),

  getDirectoryCandidates: () => apiRequest<DirectoryCandidate[]>('/api/v1/users/directory-candidates'),

  create: (input: CreateUserInput) =>
    apiRequest<{ id: string }>('/api/v1/users', { method: 'POST', body: JSON.stringify(input) }),

  update: (id: string, input: UpdateUserInput) =>
    apiRequest<void>(`/api/v1/users/${id}`, { method: 'PUT', body: JSON.stringify(input) }),

  setTeams: (id: string, teamIds: string[]) =>
    apiRequest<void>(`/api/v1/users/${id}/teams`, { method: 'PUT', body: JSON.stringify({ teamIds }) }),

  setGroups: (id: string, groupIds: string[]) =>
    apiRequest<void>(`/api/v1/users/${id}/groups`, { method: 'PUT', body: JSON.stringify({ groupIds }) }),

  deactivate: (id: string) => apiRequest<void>(`/api/v1/users/${id}/deactivate`, { method: 'POST' }),

  reactivate: (id: string) => apiRequest<void>(`/api/v1/users/${id}/reactivate`, { method: 'POST' }),
}
