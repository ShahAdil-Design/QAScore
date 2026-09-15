import { create } from 'zustand'

type Theme = 'light' | 'dark'

interface ThemeStore {
  theme: Theme
  toggle: () => void
}

function read(): Theme {
  try {
    const t = localStorage.getItem('theme')
    if (t === 'dark' || t === 'light') return t
  } catch {
    /* private mode */
  }
  return window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

function apply(t: Theme) {
  document.documentElement.setAttribute('data-theme', t)
  try {
    localStorage.setItem('theme', t)
  } catch {
    /* ignore */
  }
}

export const useTheme = create<ThemeStore>((set, get) => ({
  theme: read(),
  toggle: () => {
    const next: Theme = get().theme === 'dark' ? 'light' : 'dark'
    apply(next)
    set({ theme: next })
  },
}))
