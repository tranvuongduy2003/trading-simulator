import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

export function ChartPlaceholder() {
  return (
    <Card className="border-dashed">
      <CardHeader className="pb-3">
        <CardTitle className="text-base">Chart</CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-muted-foreground text-sm">
          Candlestick chart and timeframe controls will appear here.
        </p>
      </CardContent>
    </Card>
  )
}

export function OrderFormPlaceholder() {
  return (
    <Card className="border-dashed">
      <CardHeader className="pb-3">
        <CardTitle className="text-base">Order form</CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-muted-foreground text-sm">
          Limit and market order entry will appear here.
        </p>
      </CardContent>
    </Card>
  )
}
