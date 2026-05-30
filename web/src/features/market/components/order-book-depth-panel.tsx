import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import type { OrderBookSnapshotResponse } from '@/features/market/api'
import { OrderBookDepthTable } from '@/features/market/components/order-book-depth-table'
import { getDepthSideEmptyMessage } from '@/features/market/order-book-depth-display'
import { MARKET_LOAD_ERROR_MESSAGE } from '@/features/market/top-of-book-display'

type OrderBookDepthPanelProps = {
  isPending: boolean
  isError: boolean
  snapshot: OrderBookSnapshotResponse | null
  showReconnectingBadge?: boolean
  onRetry?: () => void
}

export function OrderBookDepthPanel({
  isPending,
  isError,
  snapshot,
  showReconnectingBadge = false,
  onRetry,
}: OrderBookDepthPanelProps) {
  const symbolLabel = snapshot?.symbol ?? 'AAPL'

  return (
    <Card className="min-w-0">
      <CardHeader className="pb-3">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <CardTitle className="text-base">
            Order book depth{' '}
            <span className="text-muted-foreground font-normal">({symbolLabel})</span>
          </CardTitle>
          {showReconnectingBadge ? (
            <span
              className="bg-muted text-muted-foreground rounded-md px-2 py-0.5 text-xs font-medium"
              aria-live="polite"
            >
              Reconnecting…
            </span>
          ) : null}
        </div>
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
          <div className="flex flex-col gap-4 lg:grid lg:grid-cols-2">
            <OrderBookDepthTable
              side="bid"
              levels={snapshot.bids}
              emptyMessage={getDepthSideEmptyMessage(snapshot.bids, snapshot.asks, 'bid')}
            />
            <OrderBookDepthTable
              side="ask"
              levels={snapshot.asks}
              emptyMessage={getDepthSideEmptyMessage(snapshot.bids, snapshot.asks, 'ask')}
            />
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
