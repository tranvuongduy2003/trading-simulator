import type { OrderBookSnapshotResponse } from '@/features/market/api'
import { formatPrice } from '@/features/market/format-price'

export type TopOfBookTouch = {
  price: number
  quantity: number
} | null

export type TopOfBookDisplay = {
  symbol: string
  bestBid: TopOfBookTouch
  bestAsk: TopOfBookTouch
}

const missingValueLabel = '—'

export function deriveTopOfBook(snapshot: OrderBookSnapshotResponse): TopOfBookDisplay {
  const bestBidLevel = snapshot.bids[0]
  const bestAskLevel = snapshot.asks[0]

  return {
    symbol: snapshot.symbol,
    bestBid: bestBidLevel ? { price: bestBidLevel.price, quantity: bestBidLevel.quantity } : null,
    bestAsk: bestAskLevel ? { price: bestAskLevel.price, quantity: bestAskLevel.quantity } : null,
  }
}

export function formatTopOfBookPrice(touch: TopOfBookTouch): string {
  if (!touch) {
    return missingValueLabel
  }

  return formatPrice(touch.price)
}

export function formatTopOfBookQuantity(touch: TopOfBookTouch): string | null {
  if (!touch || touch.quantity <= 0) {
    return null
  }

  return `${touch.quantity.toLocaleString('en-US')} shares`
}

export const MARKET_LOAD_ERROR_MESSAGE = 'Market data unavailable'
