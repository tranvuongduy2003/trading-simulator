import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import type { RecentTradesResponse } from '@/features/market/api'
import { TradeTapeTable } from '@/features/market/components/trade-tape-table'
import { MARKET_LOAD_ERROR_MESSAGE } from '@/features/market/top-of-book-display'

type TradeTapePanelProps = {
  isPending: boolean
  isError: boolean
  snapshot: RecentTradesResponse | null
  onRetry?: () => void
}

export function TradeTapePanel({ isPending, isError, snapshot, onRetry }: TradeTapePanelProps) {
  const symbolLabel = snapshot?.symbol ?? 'AAPL'

  return (
    <Card className="min-w-0">
      <CardHeader className="pb-3">
        <CardTitle className="text-base">
          Trade tape <span className="text-muted-foreground font-normal">({symbolLabel})</span>
        </CardTitle>
      </CardHeader>
      <CardContent>
        {isPending ? (
          <div className="flex flex-col gap-2" aria-busy="true">
            <Skeleton className="h-5 w-full" />
            <Skeleton className="h-32 w-full" />
          </div>
        ) : null}

        {isError ? (
          <div className="flex flex-col gap-3" role="alert">
            <p className="text-destructive text-sm">{MARKET_LOAD_ERROR_MESSAGE}</p>
            {onRetry ? (
              <Button type="button" variant="outline" size="sm" className="w-fit" onClick={onRetry}>
                Retry
              </Button>
            ) : null}
          </div>
        ) : null}

        {!isPending && !isError && snapshot ? (
          <div className="max-h-64 overflow-y-auto">
            <TradeTapeTable snapshot={snapshot} />
          </div>
        ) : null}
      </CardContent>
    </Card>
  )
}
