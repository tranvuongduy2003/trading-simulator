import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import type { OpenOrderDto } from '@/features/orders/api'
import { useOpenOrdersQuery } from '@/features/orders/hooks/use-open-orders-query'
import { useOrderHistoryQuery } from '@/features/orders/hooks/use-order-history-query'
import { useTradeHistoryQuery } from '@/features/trades/hooks/use-trade-history-query'
import type { components } from '@/lib/api'
import { formatUsd, toNumber } from '@/lib/format'

import { usePortfolioQuery } from '../hooks/use-portfolio-query'

const AAPL_SYMBOL = 'AAPL'

const PORTFOLIO_LOAD_ERROR_MESSAGE = 'Could not load holdings. Try refreshing.'
const OPEN_ORDERS_LOAD_ERROR_MESSAGE = 'Could not load open orders. Try refreshing.'
const ORDER_HISTORY_LOAD_ERROR_MESSAGE = 'Could not load order history. Try refreshing.'
const TRADE_HISTORY_LOAD_ERROR_MESSAGE = 'Could not load trade history. Try refreshing.'

type OrderHistoryItemDto = components['schemas']['OrderHistoryItemDto']
type TradeHistoryItemDto = components['schemas']['TradeHistoryItemDto']

function formatDateTime(value: string) {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }

  return new Intl.DateTimeFormat('en-US', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(date)
}

function formatOrderPrice(price: null | number | string) {
  if (price === null) {
    return 'Market'
  }

  return formatUsd(toNumber(price))
}

function formatSide(side: string) {
  return side.charAt(0).toUpperCase() + side.slice(1).toLowerCase()
}

function ActivityTableSkeleton({ rows = 3 }: { rows?: number }) {
  return (
    <div className="flex flex-col gap-2" aria-busy="true">
      {Array.from({ length: rows }, (_, index) => (
        <Skeleton key={index} className="h-4 w-full" />
      ))}
    </div>
  )
}

function ActivityEmptyState({ message }: { message: string }) {
  return <p className="text-muted-foreground text-sm">{message}</p>
}

function ActivityErrorState({ message }: { message: string }) {
  return (
    <p className="text-destructive text-sm" role="alert">
      {message}
    </p>
  )
}

function HoldingsPanel() {
  const portfolioQuery = usePortfolioQuery()

  const aaplHolding = portfolioQuery.data?.holdings.find(
    (holding) => holding.symbol === AAPL_SYMBOL,
  )
  const aaplAvailable = aaplHolding ? toNumber(aaplHolding.availableQuantity) : 0

  if (portfolioQuery.isPending) {
    return <ActivityTableSkeleton />
  }

  if (portfolioQuery.isError) {
    return <ActivityErrorState message={PORTFOLIO_LOAD_ERROR_MESSAGE} />
  }

  if (!portfolioQuery.isSuccess) {
    return null
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Symbol</TableHead>
          <TableHead className="text-right">Available</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        <TableRow>
          <TableCell className="font-medium">{AAPL_SYMBOL}</TableCell>
          <TableCell className="text-right tabular-nums">{aaplAvailable}</TableCell>
        </TableRow>
      </TableBody>
    </Table>
  )
}

function OpenOrdersPanel() {
  const openOrdersQuery = useOpenOrdersQuery()

  if (openOrdersQuery.isPending) {
    return <ActivityTableSkeleton />
  }

  if (openOrdersQuery.isError) {
    return <ActivityErrorState message={OPEN_ORDERS_LOAD_ERROR_MESSAGE} />
  }

  if (!openOrdersQuery.isSuccess) {
    return null
  }

  const orders = openOrdersQuery.data

  if (orders.length === 0) {
    return <ActivityEmptyState message="No open orders" />
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Symbol</TableHead>
          <TableHead>Side</TableHead>
          <TableHead className="text-right">Remaining</TableHead>
          <TableHead className="text-right">Price</TableHead>
          <TableHead>Status</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {orders.map((order) => (
          <OpenOrderRow key={order.orderId} order={order} />
        ))}
      </TableBody>
    </Table>
  )
}

function OpenOrderRow({ order }: { order: OpenOrderDto }) {
  return (
    <TableRow>
      <TableCell className="font-medium">{order.symbol}</TableCell>
      <TableCell>{formatSide(order.side)}</TableCell>
      <TableCell className="text-right tabular-nums">{toNumber(order.remainingQuantity)}</TableCell>
      <TableCell className="text-right tabular-nums">{formatOrderPrice(order.price)}</TableCell>
      <TableCell>{order.status}</TableCell>
    </TableRow>
  )
}

function OrderHistoryPanel() {
  const orderHistoryQuery = useOrderHistoryQuery()

  if (orderHistoryQuery.isPending) {
    return <ActivityTableSkeleton />
  }

  if (orderHistoryQuery.isError) {
    return <ActivityErrorState message={ORDER_HISTORY_LOAD_ERROR_MESSAGE} />
  }

  if (!orderHistoryQuery.isSuccess) {
    return null
  }

  const items = orderHistoryQuery.data.items

  if (items.length === 0) {
    return <ActivityEmptyState message="No orders yet" />
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Symbol</TableHead>
          <TableHead>Side</TableHead>
          <TableHead className="text-right">Qty</TableHead>
          <TableHead className="text-right">Price</TableHead>
          <TableHead>Status</TableHead>
          <TableHead className="text-right">Created</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {items.map((item) => (
          <OrderHistoryRow key={item.orderId} item={item} />
        ))}
      </TableBody>
    </Table>
  )
}

function OrderHistoryRow({ item }: { item: OrderHistoryItemDto }) {
  return (
    <TableRow>
      <TableCell className="font-medium">{item.symbol}</TableCell>
      <TableCell>{formatSide(item.side)}</TableCell>
      <TableCell className="text-right tabular-nums">{toNumber(item.originalQuantity)}</TableCell>
      <TableCell className="text-right tabular-nums">{formatOrderPrice(item.price)}</TableCell>
      <TableCell>{item.status}</TableCell>
      <TableCell className="text-muted-foreground text-right text-xs tabular-nums">
        {formatDateTime(item.createdAt)}
      </TableCell>
    </TableRow>
  )
}

function TradeHistoryPanel() {
  const tradeHistoryQuery = useTradeHistoryQuery()

  if (tradeHistoryQuery.isPending) {
    return <ActivityTableSkeleton />
  }

  if (tradeHistoryQuery.isError) {
    return <ActivityErrorState message={TRADE_HISTORY_LOAD_ERROR_MESSAGE} />
  }

  if (!tradeHistoryQuery.isSuccess) {
    return null
  }

  const items = tradeHistoryQuery.data.items

  if (items.length === 0) {
    return <ActivityEmptyState message="No trades yet" />
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Symbol</TableHead>
          <TableHead>Side</TableHead>
          <TableHead className="text-right">Qty</TableHead>
          <TableHead className="text-right">Price</TableHead>
          <TableHead className="text-right">Executed</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {items.map((item) => (
          <TradeHistoryRow key={item.tradeId} item={item} />
        ))}
      </TableBody>
    </Table>
  )
}

function TradeHistoryRow({ item }: { item: TradeHistoryItemDto }) {
  return (
    <TableRow>
      <TableCell className="font-medium">{item.symbol}</TableCell>
      <TableCell>{formatSide(item.side)}</TableCell>
      <TableCell className="text-right tabular-nums">{toNumber(item.quantity)}</TableCell>
      <TableCell className="text-right tabular-nums">{formatUsd(toNumber(item.price))}</TableCell>
      <TableCell className="text-muted-foreground text-right text-xs tabular-nums">
        {formatDateTime(item.executedAt)}
      </TableCell>
    </TableRow>
  )
}

export function PortfolioActivityTabs() {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Portfolio activity</CardTitle>
      </CardHeader>
      <CardContent>
        <Tabs defaultValue="open-orders">
          <TabsList className="w-full flex-wrap">
            <TabsTrigger value="open-orders">Open Orders</TabsTrigger>
            <TabsTrigger value="order-history">Order History</TabsTrigger>
            <TabsTrigger value="trade-history">Trade History</TabsTrigger>
            <TabsTrigger value="holdings">Holdings</TabsTrigger>
          </TabsList>

          <TabsContent value="open-orders" className="mt-4">
            <OpenOrdersPanel />
          </TabsContent>

          <TabsContent value="order-history" className="mt-4">
            <OrderHistoryPanel />
          </TabsContent>

          <TabsContent value="trade-history" className="mt-4">
            <TradeHistoryPanel />
          </TabsContent>

          <TabsContent value="holdings" className="mt-4">
            <HoldingsPanel />
          </TabsContent>
        </Tabs>
      </CardContent>
    </Card>
  )
}
