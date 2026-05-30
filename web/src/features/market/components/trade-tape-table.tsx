import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import type { RecentTradesResponse } from '@/features/market/api'
import { formatPrice } from '@/features/market/format-price'
import { formatTapeQuantity, formatTapeTime } from '@/features/market/trade-tape-display'

type TradeTapeTableProps = {
  snapshot: RecentTradesResponse
}

export function TradeTapeTable({ snapshot }: TradeTapeTableProps) {
  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Time</TableHead>
          <TableHead className="text-right">Price</TableHead>
          <TableHead className="text-right">Size</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {snapshot.trades.map((trade) => (
          <TableRow key={trade.tradeIdentifier}>
            <TableCell className="tabular-nums">{formatTapeTime(trade.executedAt)}</TableCell>
            <TableCell className="text-right tabular-nums">{formatPrice(trade.price)}</TableCell>
            <TableCell className="text-right tabular-nums">
              {formatTapeQuantity(trade.quantity)}
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  )
}
