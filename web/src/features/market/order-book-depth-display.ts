import type { OrderBookLevelResponse } from '@/features/market/api'
import { formatPrice } from '@/features/market/format-price'

export function formatDepthPrice(level: OrderBookLevelResponse): string {
  return formatPrice(level.price)
}

export function formatDepthQuantity(quantity: number): string {
  return quantity.toLocaleString('en-US')
}

export function formatDepthOrderCount(orderCount: number): string {
  return orderCount.toLocaleString('en-US')
}
