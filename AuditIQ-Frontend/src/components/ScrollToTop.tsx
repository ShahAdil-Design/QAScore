import { useEffect } from 'react'
import { useLocation } from 'react-router-dom'

/** SPA navigation doesn't reset scroll position the way a full page load does — without this,
 * navigating to a page like Edit Scorecard leaves the viewport wherever it happened to be on
 * the page you navigated from. */
export function ScrollToTop() {
  const { pathname } = useLocation()

  useEffect(() => {
    window.scrollTo(0, 0)
  }, [pathname])

  return null
}
