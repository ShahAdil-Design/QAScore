import { create } from 'zustand'

interface BreadcrumbStore {
  extra: string | null
  setExtra: (label: string | null) => void
}

/** Lets a page add a third breadcrumb segment (e.g. "Workspace / Scorecards / {name}") without
 * the shared Header needing to know about page-specific data. Pages set this on mount and clear
 * it on unmount so it never leaks onto an unrelated page. */
export const useBreadcrumb = create<BreadcrumbStore>((set) => ({
  extra: null,
  setExtra: (extra) => set({ extra }),
}))
