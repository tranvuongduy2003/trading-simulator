import type { OrderBookLevelResponse } from '@/features/market/api'
import { formatPrice } from '@/features/market/format-price'
import { EMPTY_BOOK_MESSAGE } from '@/features/market/top-of-book-display'

const noBidsLabel = 'No bids'
const noAsksLabel = 'No asks'

export function getDepthSideEmptyMessage(
  bids: readonly OrderBookLevelResponse[],
  asks: readonly OrderBookLevelResponse[],
  side: 'bid' | 'ask',
): string | null {
  const levels = side === 'bid' ? bids : asks
  if (levels.length > 0) {
    return null
  }

  if (bids.length === 0 && asks.length === 0) {
    return EMPTY_BOOK_MESSAGE
  }

  return side === 'bid' ? noBidsLabel : noAsksLabel
}

export function formatDepthPrice(level: OrderBookLevelResponse): string {
  return formatPrice(level.price)
}

export function formatDepthQuantity(quantity: number): string {
  return quantity.toLocaleString('en-US')
}

export function formatDepthOrderCount(orderCount: number): string {
  return orderCount.toLocaleString('en-US')
}
