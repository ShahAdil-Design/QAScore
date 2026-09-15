import type { ReactNode } from 'react'

interface DataTableProps {
  headers: string[]
  rows: ReactNode[][]
  emptyLabel?: string
}

export function DataTable({ headers, rows, emptyLabel = 'No records yet' }: DataTableProps) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[680px] text-left text-sm">
        <thead className="bg-muted/40 text-xs uppercase tracking-wide text-muted-foreground">
          <tr>
            {headers.map((h) => (
              <th key={h} className="px-5 py-3 font-medium">
                {h}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-border">
          {rows.length === 0 ? (
            <tr>
              <td colSpan={headers.length} className="px-5 py-8 text-center text-muted-foreground">
                {emptyLabel}
              </td>
            </tr>
          ) : (
            rows.map((row, i) => (
              <tr key={i} className="hover:bg-muted/30">
                {row.map((cell, j) => (
                  <td key={j} className="px-5 py-4 text-muted-foreground">
                    {cell}
                  </td>
                ))}
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  )
}
