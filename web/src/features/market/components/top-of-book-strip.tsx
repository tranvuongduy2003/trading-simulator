import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import type { TopOfBookDisplay } from '@/features/market/top-of-book-display'
import {
  formatTopOfBookPrice,
  formatTopOfBookQuantity,
  MARKET_LOAD_ERROR_MESSAGE,
} from '@/features/market/top-of-book-display'

type TopOfBookStripProps = {
  isPending: boolean
  isError: boolean
  display: TopOfBookDisplay | null
  showReconnecting?: boolean
  onRetry?: () => void
}

export function TopOfBookStrip({
  isPending,
  isError,
  display,
  showReconnecting = false,
  onRetry,
}: TopOfBookStripProps) {
  const symbolLabel = display?.symbol ?? 'AAPL'

  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="flex flex-wrap items-center gap-2">
          <CardTitle className="text-base">
            Top of book <span className="text-muted-foreground font-normal">({symbolLabel})</span>
          </CardTitle>
          {showReconnecting ? (
            <span className="text-muted-foreground text-xs" aria-live="polite">
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
          <div
            className="grid gap-3 sm:grid-cols-2"
            aria-live="polite"
            aria-label={`Best bid and best ask for ${symbolLabel}`}
          >
            <TopOfBookSide
              label="Best bid"
              touch={display.bestBid}
              valueClassName="text-bid tabular-nums"
            />
            <TopOfBookSide
              label="Best ask"
              touch={display.bestAsk}
              valueClassName="text-ask tabular-nums"
            />
          </div>
        ) : null}
      </CardContent>
    </Card>
  )
}

type TopOfBookSideProps = {
  label: string
  touch: TopOfBookDisplay['bestBid']
  valueClassName: string
}

function TopOfBookSide({ label, touch, valueClassName }: TopOfBookSideProps) {
  const quantityLabel = formatTopOfBookQuantity(touch)

  return (
    <div className="bg-muted/40 flex flex-col gap-1 rounded-md border p-3">
      <p className="text-muted-foreground text-xs font-medium tracking-wide uppercase">{label}</p>
      <p className={`text-2xl font-semibold ${valueClassName}`}>{formatTopOfBookPrice(touch)}</p>
      {quantityLabel ? (
        <p className="text-muted-foreground text-xs tabular-nums">{quantityLabel}</p>
      ) : null}
    </div>
  )
}
