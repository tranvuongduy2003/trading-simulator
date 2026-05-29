import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import type { OrderBookSnapshotResponse } from '@/features/market/api'
import { OrderBookDepthTable } from '@/features/market/components/order-book-depth-table'
import { MARKET_LOAD_ERROR_MESSAGE } from '@/features/market/top-of-book-display'

type OrderBookDepthPanelProps = {
  isPending: boolean
  isError: boolean
  snapshot: OrderBookSnapshotResponse | null
  onRetry?: () => void
}

export function OrderBookDepthPanel({
  isPending,
  isError,
  snapshot,
  onRetry,
}: OrderBookDepthPanelProps) {
  const symbolLabel = snapshot?.symbol ?? 'AAPL'

  return (
    <Card>
      <CardHeader className="pb-3">
        <CardTitle className="text-base">
          Order book depth{' '}
          <span className="text-muted-foreground font-normal">({symbolLabel})</span>
        </CardTitle>
      </CardHeader>
      <CardContent>
        {isPending ? (
          <div className="flex flex-col gap-4 lg:grid lg:grid-cols-2" aria-busy="true">
            <DepthTableSkeleton />
            <DepthTableSkeleton />
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
          <div className="flex flex-col gap-4 lg:grid lg:grid-cols-2" aria-live="polite">
            <OrderBookDepthTable side="bid" levels={snapshot.bids} />
            <OrderBookDepthTable side="ask" levels={snapshot.asks} />
          </div>
        ) : null}
      </CardContent>
    </Card>
  )
}

function DepthTableSkeleton() {
  return (
    <div className="flex flex-col gap-2">
      <Skeleton className="h-5 w-16" />
      <Skeleton className="h-40 w-full" />
    </div>
  )
}
