import type { ButtonHTMLAttributes } from 'react'
import clsx from 'clsx'

type ButtonVariant = 'primary' | 'secondary' | 'ghost'

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant
}

const variantClass: Record<ButtonVariant, string> = {
  primary: 'bg-primary text-primary-foreground hover:bg-primary/90',
  secondary: 'border border-input bg-card hover:bg-muted',
  ghost: 'text-muted-foreground hover:bg-muted hover:text-foreground',
}

export function Button({ variant = 'primary', className, ...props }: ButtonProps) {
  return (
    <button
      className={clsx(
        'inline-flex items-center justify-center gap-2 rounded-lg px-3 py-2 text-sm font-medium shadow-sm transition-colors disabled:pointer-events-none disabled:opacity-50',
        variantClass[variant],
        className,
      )}
      {...props}
    />
  )
}
