import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import type { OrderBookLevelResponse } from '@/features/market/api'
import {
  formatDepthOrderCount,
  formatDepthPrice,
  formatDepthQuantity,
} from '@/features/market/order-book-depth-display'

type OrderBookDepthTableProps = {
  side: 'bid' | 'ask'
  levels: OrderBookLevelResponse[]
}

export function OrderBookDepthTable({ side, levels }: OrderBookDepthTableProps) {
  const sideLabel = side === 'bid' ? 'Bids' : 'Asks'
  const priceClassName = side === 'bid' ? 'text-bid tabular-nums' : 'text-ask tabular-nums'

  return (
    <div className="flex flex-col gap-2">
      <h3 className={`text-sm font-medium ${side === 'bid' ? 'text-bid' : 'text-ask'}`}>
        {sideLabel}
      </h3>
      <Table aria-label={`${sideLabel} depth levels`}>
        <TableHeader>
          <TableRow>
            <TableHead>Price</TableHead>
            <TableHead className="text-right">Size</TableHead>
            <TableHead className="text-right">Orders</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {levels.map((level) => (
            <TableRow key={`${side}-${level.price}`}>
              <TableCell className={priceClassName}>{formatDepthPrice(level)}</TableCell>
              <TableCell className="text-right tabular-nums">
                {formatDepthQuantity(level.quantity)}
              </TableCell>
              <TableCell className="text-right tabular-nums">
                {formatDepthOrderCount(level.orderCount)}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}
