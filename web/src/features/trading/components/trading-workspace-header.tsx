import { Badge } from '@/components/ui/badge'

export function TradingWorkspaceHeader() {
  return (
    <div className="flex flex-wrap items-center gap-3">
      <h1 className="text-2xl font-semibold tracking-tight">Trading</h1>
      <Badge variant="secondary" aria-label="Symbol AAPL">
        AAPL
      </Badge>
    </div>
  )
}
