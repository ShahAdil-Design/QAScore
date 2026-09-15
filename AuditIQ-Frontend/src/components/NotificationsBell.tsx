import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Bell, CheckCheck } from 'lucide-react'
import clsx from 'clsx'
import { notificationsApi, type Notification } from '@/api/notifications'

const typeRoute: Record<Notification['type'], (n: Notification) => string | null> = {
  EvaluationSubmitted: (n) => (n.relatedEvaluationId ? `/review/${n.relatedEvaluationId}` : null),
  DisputeResolved: (n) => (n.relatedEvaluationId ? `/review/${n.relatedEvaluationId}` : null),
  CalibrationListAssigned: (n) => (n.relatedCalibrationListId ? `/calibration/${n.relatedCalibrationListId}/items` : null),
}

function timeAgo(iso: string) {
  const seconds = Math.floor((Date.now() - new Date(iso).getTime()) / 1000)
  if (seconds < 60) return 'just now'
  const minutes = Math.floor(seconds / 60)
  if (minutes < 60) return `${minutes}m ago`
  const hours = Math.floor(minutes / 60)
  if (hours < 24) return `${hours}h ago`
  return `${Math.floor(hours / 24)}d ago`
}

/**
 * Header bell — polls the unread count regardless of whether the dropdown is open (so the badge
 * stays live), and only fetches the actual list once the user opens it.
 */
export function NotificationsBell() {
  const [isOpen, setIsOpen] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  useEffect(() => {
    if (!isOpen) return
    function handleClickOutside(e: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) setIsOpen(false)
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [isOpen])

  const unreadCountQuery = useQuery({
    queryKey: ['notifications', 'unread-count'],
    queryFn: () => notificationsApi.unreadCount(),
    refetchInterval: 30_000,
  })

  const listQuery = useQuery({
    queryKey: ['notifications', 'list'],
    queryFn: () => notificationsApi.list(),
    enabled: isOpen,
  })

  function invalidate() {
    queryClient.invalidateQueries({ queryKey: ['notifications'] })
  }

  const markReadMutation = useMutation({
    mutationFn: (id: string) => notificationsApi.markRead(id),
    onSuccess: invalidate,
  })

  const markAllReadMutation = useMutation({
    mutationFn: () => notificationsApi.markAllRead(),
    onSuccess: invalidate,
  })

  function handleClick(notification: Notification) {
    if (!notification.isRead) markReadMutation.mutate(notification.id)
    const route = typeRoute[notification.type](notification)
    if (route) {
      navigate(route)
      setIsOpen(false)
    }
  }

  const unreadCount = unreadCountQuery.data ?? 0

  return (
    <div ref={containerRef} className="relative">
      <button
        onClick={() => setIsOpen((v) => !v)}
        aria-label="Notifications"
        title="Notifications"
        className="relative rounded-lg p-2 text-muted-foreground hover:bg-muted"
      >
        <Bell className="size-[18px]" />
        {unreadCount > 0 && (
          <span className="absolute right-1 top-1 flex size-4 items-center justify-center rounded-full bg-status-fail text-[10px] font-medium text-white">
            {unreadCount > 9 ? '9+' : unreadCount}
          </span>
        )}
      </button>

      {isOpen && (
        <div className="absolute right-0 z-40 mt-2 w-80 rounded-xl border border-border bg-card shadow-lg">
          <div className="flex items-center justify-between border-b border-border px-4 py-3">
            <span className="text-sm font-semibold">Notifications</span>
            {unreadCount > 0 && (
              <button
                onClick={() => markAllReadMutation.mutate()}
                disabled={markAllReadMutation.isPending}
                className="flex items-center gap-1 text-xs text-primary hover:underline disabled:opacity-60"
              >
                <CheckCheck className="size-3.5" />
                Mark all read
              </button>
            )}
          </div>

          <div className="max-h-96 overflow-y-auto">
            {listQuery.isLoading && (
              <p className="p-4 text-sm text-muted-foreground">Loading...</p>
            )}
            {listQuery.isError && (
              <p className="p-4 text-sm text-status-fail">Failed to load notifications.</p>
            )}
            {listQuery.data && listQuery.data.items.length === 0 && (
              <p className="p-4 text-sm text-muted-foreground">You're all caught up.</p>
            )}
            {listQuery.data?.items.map((notification) => (
              <button
                key={notification.id}
                onClick={() => handleClick(notification)}
                className={clsx(
                  'flex w-full flex-col gap-1 border-b border-border px-4 py-3 text-left last:border-b-0 hover:bg-muted',
                  !notification.isRead && 'bg-primary/5',
                )}
              >
                <div className="flex items-start gap-2">
                  {!notification.isRead && <span className="mt-1.5 size-1.5 shrink-0 rounded-full bg-primary" />}
                  <span className={clsx('text-sm', !notification.isRead && 'font-medium')}>{notification.message}</span>
                </div>
                <span className="pl-3.5 text-xs text-muted-foreground">{timeAgo(notification.createdAtUtc)}</span>
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}
