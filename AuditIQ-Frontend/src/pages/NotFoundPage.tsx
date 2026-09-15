import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <div className="rounded-lg border border-border bg-card p-6 text-center">
      <h1 className="text-xl font-semibold">Page not found</h1>
      <Link to="/" className="mt-2 inline-block text-primary hover:underline">
        Back to Dashboard
      </Link>
    </div>
  )
}
