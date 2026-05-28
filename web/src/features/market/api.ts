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
  const search = new URLSearchParams({ symbol })
  return apiClient.get<OrderBookSnapshotResponse>(`/api/market/orderbook?${search.toString()}`, {
    signal,
    suppressErrorToast: true,
  })
}
