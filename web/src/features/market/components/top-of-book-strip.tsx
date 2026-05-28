import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import type { TopOfBookDisplay } from '@/features/market/top-of-book-display'
import {
  formatMidPrice,
  formatSpreadAbsolute,
  formatSpreadPercent,
  formatTopOfBookPrice,
  formatTopOfBookQuantity,
  MARKET_LOAD_ERROR_MESSAGE,
} from '@/features/market/top-of-book-display'

type TopOfBookStripProps = {
  isPending: boolean
  isError: boolean
  display: TopOfBookDisplay | null
  onRetry?: () => void
}

export function TopOfBookStrip({ isPending, isError, display, onRetry }: TopOfBookStripProps) {
  const symbolLabel = display?.symbol ?? 'AAPL'

  return (
    <Card>
      <CardHeader className="pb-3">
        <CardTitle className="text-base">
          Top of book <span className="text-muted-foreground font-normal">({symbolLabel})</span>
        </CardTitle>
      </CardHeader>
      <CardContent>
        {isPending ? (
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4" aria-busy="true">
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
          <TopOfBookValues display={display} symbolLabel={symbolLabel} />
        ) : null}
      </CardContent>
    </Card>
  )
}

type TopOfBookValuesProps = {
  display: TopOfBookDisplay
  symbolLabel: string
}

function TopOfBookValues({ display, symbolLabel }: TopOfBookValuesProps) {
  const { metrics } = display

  return (
    <div
      className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4"
      aria-live="polite"
      aria-label={`Top of book including spread and mid for ${symbolLabel}`}
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
      <TopOfBookMetricCell
        label="Spread"
        primaryValue={formatSpreadAbsolute(metrics.spreadAbsolute)}
        secondaryValue={formatSpreadPercent(metrics.spreadPercent)}
      />
      <TopOfBookMetricCell label="Mid" primaryValue={formatMidPrice(metrics.midPrice)} />
    </div>
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

type TopOfBookMetricCellProps = {
  label: string
  primaryValue: string
  secondaryValue?: string
}

function TopOfBookMetricCell({ label, primaryValue, secondaryValue }: TopOfBookMetricCellProps) {
  return (
    <div className="bg-muted/40 flex flex-col gap-1 rounded-md border p-3">
      <p className="text-muted-foreground text-xs font-medium tracking-wide uppercase">{label}</p>
      <p className="text-2xl font-semibold tabular-nums">{primaryValue}</p>
      {secondaryValue ? (
        <p className="text-muted-foreground text-xs tabular-nums">{secondaryValue}</p>
      ) : null}
    </div>
  )
}
