import { apiRequest } from './client'

export type NotificationType = 'EvaluationSubmitted' | 'DisputeResolved' | 'CalibrationListAssigned'

export interface Notification {
  id: string
  type: NotificationType
  message: string
  relatedEvaluationId: string | null
  relatedCalibrationListId: string | null
  createdAtUtc: string
  isRead: boolean
}

export interface NotificationPage {
  items: Notification[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export const notificationsApi = {
  list: (unreadOnly = false, page = 1, pageSize = 20) => {
    const params = new URLSearchParams()
    params.set('unreadOnly', String(unreadOnly))
    params.set('page', String(page))
    params.set('pageSize', String(pageSize))
    return apiRequest<NotificationPage>(`/api/v1/notifications?${params.toString()}`)
  },

  unreadCount: () => apiRequest<number>('/api/v1/notifications/unread-count'),

  markRead: (id: string) => apiRequest<void>(`/api/v1/notifications/${id}/read`, { method: 'POST' }),

  markAllRead: () => apiRequest<void>('/api/v1/notifications/read-all', { method: 'POST' }),
}
