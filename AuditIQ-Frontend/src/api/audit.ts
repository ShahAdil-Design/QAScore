import { apiRequest } from './client'

export type AuditAction = 'Created' | 'Updated' | 'Deleted'

export interface AuditLogEntry {
  id: string
  timestampUtc: string
  userId: string | null
  userDisplayName: string | null
  entityName: string
  entityId: string
  action: AuditAction
  changes: string | null
}

export interface AuditLogPage {
  items: AuditLogEntry[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface AuditLogFilters {
  entityName?: string
  userId?: string
  action?: AuditAction
  dateFrom?: string
  dateTo?: string
  page?: number
  pageSize?: number
}

export const auditApi = {
  getLog: (filters: AuditLogFilters) => {
    const params = new URLSearchParams()
    if (filters.entityName) params.set('entityName', filters.entityName)
    if (filters.userId) params.set('userId', filters.userId)
    if (filters.action) params.set('action', filters.action)
    if (filters.dateFrom) params.set('dateFrom', filters.dateFrom)
    if (filters.dateTo) params.set('dateTo', filters.dateTo)
    params.set('page', String(filters.page ?? 1))
    params.set('pageSize', String(filters.pageSize ?? 25))
    return apiRequest<AuditLogPage>(`/api/v1/audit?${params.toString()}`)
  },
}
