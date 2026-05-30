import { apiClient } from '@/lib/api'

export type OrderBookLevelResponse = {
  price: number
  quantity: number
  orderCount: number
}

export type OrderBookSnapshotResponse = {
  symbol: string
  bids: OrderBookLevelResponse[]
  asks: OrderBookLevelResponse[]
  updatedAt: string
}

export function getOrderBook(symbol: string, signal?: AbortSignal) {
  const search = new URLSearchParams({ symbol, depth: '10' })
  return apiClient.get<OrderBookSnapshotResponse>(`/api/market/orderbook?${search.toString()}`, {
    signal,
    suppressErrorToast: true,
  })
}

export type RecentTradeItemResponse = {
  tradeIdentifier: string
  price: number
  quantity: number
  executedAt: string
}

export type RecentTradesResponse = {
  symbol: string
  trades: RecentTradeItemResponse[]
  updatedAt: string
}

export function getRecentTrades(symbol: string, limit: number, signal?: AbortSignal) {
  const search = new URLSearchParams({ symbol, limit: String(limit) })
  return apiClient.get<RecentTradesResponse>(`/api/market/trades?${search.toString()}`, {
    signal,
    suppressErrorToast: true,
  })
}
