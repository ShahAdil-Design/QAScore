import type { HTMLAttributes } from 'react'
import clsx from 'clsx'

type BadgeTone = 'pass' | 'fail' | 'warn' | 'neutral' | 'primary'

const toneClass: Record<BadgeTone, string> = {
  pass: 'bg-status-pass-bg text-status-pass',
  fail: 'bg-status-fail-bg text-status-fail',
  warn: 'bg-status-warn-bg text-status-warn',
  neutral: 'bg-muted text-muted-foreground',
  primary: 'bg-primary/10 text-primary',
}

interface BadgeProps extends HTMLAttributes<HTMLSpanElement> {
  tone?: BadgeTone
}

export function Badge({ tone = 'neutral', className, ...props }: BadgeProps) {
  return (
    <span
      className={clsx('rounded-full px-2 py-1 text-xs font-medium', toneClass[tone], className)}
      {...props}
    />
  )
}
