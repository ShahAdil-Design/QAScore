import { ChevronDown } from 'lucide-react'
import { SectionHeader } from '@/components/ui/SectionHeader'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'

const teams = [
  { team: 'Support', score: '93.4%', change: '+8.4%' },
  { team: 'Sales', score: '88.1%', change: '+3.2%' },
  { team: 'Retention', score: '86.7%', change: '+5.8%' },
]

const drivers = ['Empathy and tone', 'Resolution accuracy', 'Clear next steps', 'Discovery questions']

export function ReportsPage() {
  return (
    <>
      <SectionHeader
        title="Reports"
        description="Standard reporting suite via Databricks (Epic 6). Data shown is placeholder — blocked on the Databricks connection method decision (Section 7)."
        action={
          <Button variant="secondary">
            <ChevronDown className="size-4" />
            Last 30 days
          </Button>
        }
      />

      <div className="grid gap-4 sm:grid-cols-3">
        {teams.map((t) => (
          <Card key={t.team}>
            <div className="text-sm text-muted-foreground">{t.team}</div>
            <div className="mt-3 text-2xl font-semibold">{t.score}</div>
            <div className="mt-1 text-xs font-medium text-status-pass">{t.change} vs prior period</div>
          </Card>
        ))}
      </div>

      <Card className="mt-6">
        <h3 className="font-semibold">Top quality drivers</h3>
        <div className="mt-5 flex flex-col gap-4">
          {drivers.map((item, i) => (
            <div key={item} className="flex items-center gap-4">
              <span className="flex size-8 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
                {i + 1}
              </span>
              <span className="flex-1 text-sm font-medium">{item}</span>
              <div className="h-2 w-40 rounded-full bg-muted">
                <div className="h-2 rounded-full bg-primary" style={{ width: `${92 - i * 13}%` }} />
              </div>
              <span className="w-10 text-right text-sm font-semibold">{92 - i * 13}%</span>
            </div>
          ))}
        </div>
      </Card>
    </>
  )
}
