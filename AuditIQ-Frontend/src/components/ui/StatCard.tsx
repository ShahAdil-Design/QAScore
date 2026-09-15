import type { LucideIcon } from 'lucide-react'
import { ArrowUpRight, ArrowDownRight } from 'lucide-react'
import { Card } from './Card'

interface StatCardProps {
  label: string
  value: string
  change?: string
  trend?: 'up' | 'down'
  icon?: LucideIcon
}

export function StatCard({ label, value, change, trend = 'up', icon: Icon }: StatCardProps) {
  const TrendIcon = trend === 'up' ? ArrowUpRight : ArrowDownRight

  return (
    <Card>
      <div className="flex items-center justify-between text-sm text-muted-foreground">
        {label}
        {Icon && <Icon className="size-4 text-primary" />}
      </div>
      <div className="mt-4 text-2xl font-semibold">{value}</div>
      {change && (
        <div
          className={
            'mt-1 flex items-center gap-1 text-xs font-medium ' +
            (trend === 'up' ? 'text-status-pass' : 'text-status-fail')
          }
        >
          <TrendIcon className="size-3" />
          {change}
        </div>
      )}
    </Card>
  )
}
