import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import type { TopOfBookDisplay } from '@/features/market/top-of-book-display'
import {
  deriveBookLiquidityState,
  EMPTY_BOOK_MESSAGE,
  formatMidPrice,
  formatSpreadAbsolute,
  formatSpreadPercent,
  formatTopOfBookQuantity,
  formatTopOfBookSidePrice,
  MARKET_LOAD_ERROR_MESSAGE,
} from '@/features/market/top-of-book-display'

type TopOfBookStripProps = {
  isPending: boolean
  isError: boolean
  display: TopOfBookDisplay | null
  showReconnectingBadge?: boolean
  onRetry?: () => void
}

export function TopOfBookStrip({
  isPending,
  isError,
  display,
  showReconnectingBadge = false,
  onRetry,
}: TopOfBookStripProps) {
  const symbolLabel = display?.symbol ?? 'AAPL'
  const spreadPercentLabel = display ? formatSpreadPercent(display.spread) : null
  const liquidityState = display ? deriveBookLiquidityState(display) : null
  const showEmptyBookBanner = liquidityState === 'empty'

  return (
    <Card className="min-w-0">
      <CardHeader className="pb-3">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <CardTitle className="text-base">
            Top of book <span className="text-muted-foreground font-normal">({symbolLabel})</span>
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
          <div className="grid gap-3 sm:grid-cols-2" aria-busy="true">
            <Skeleton className="h-16 w-full" />
            <Skeleton className="h-16 w-full" />
            <Skeleton className="h-16 w-full" />
            <Skeleton className="h-16 w-full" />
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

        {!isPending && !isError && display ? (
          <div className="flex flex-col gap-3">
            {showEmptyBookBanner ? (
              <p className="text-muted-foreground text-sm" aria-live="polite">
                {EMPTY_BOOK_MESSAGE}
              </p>
            ) : null}
            <div
              className="grid gap-3 sm:grid-cols-2"
              aria-live="polite"
              aria-label={`Best bid, best ask, spread, and mid for ${symbolLabel}`}
            >
              <TopOfBookSide
                label="Best bid"
                touch={display.bestBid}
                side="bid"
                valueClassName="text-bid tabular-nums"
              />
              <TopOfBookSide
                label="Best ask"
                touch={display.bestAsk}
                side="ask"
                valueClassName="text-ask tabular-nums"
              />
              <MetricCell label="Spread">
                <p className="text-foreground text-2xl font-semibold tabular-nums">
                  {formatSpreadAbsolute(display.spread)}
                </p>
                {spreadPercentLabel ? (
                  <p className="text-muted-foreground text-xs tabular-nums">{spreadPercentLabel}</p>
                ) : null}
              </MetricCell>
              <MetricCell label="Mid">
                <p className="text-muted-foreground text-2xl font-semibold tabular-nums">
                  {formatMidPrice(display.midPrice)}
                </p>
              </MetricCell>
            </div>
          </div>
        ) : null}
      </CardContent>
    </Card>
  )
}

type TopOfBookSideProps = {
  label: string
  touch: TopOfBookDisplay['bestBid']
  side: 'bid' | 'ask'
  valueClassName: string
}

function TopOfBookSide({ label, touch, side, valueClassName }: TopOfBookSideProps) {
  const quantityLabel = formatTopOfBookQuantity(touch)
  const priceLabel = formatTopOfBookSidePrice(touch, side)
  const isEmptySideCopy = touch === null

  return (
    <MetricCell label={label}>
      <p
        className={`text-2xl font-semibold ${isEmptySideCopy ? 'text-muted-foreground' : valueClassName}`}
      >
        {priceLabel}
      </p>
      {quantityLabel ? (
        <p className="text-muted-foreground text-xs tabular-nums">{quantityLabel}</p>
      ) : null}
    </MetricCell>
  )
}

type MetricCellProps = {
  label: string
  children: React.ReactNode
}

function MetricCell({ label, children }: MetricCellProps) {
  return (
    <div className="bg-muted/40 flex flex-col gap-1 rounded-md border p-3">
      <p className="text-muted-foreground text-xs font-medium tracking-wide uppercase">{label}</p>
      {children}
    </div>
  )
}
