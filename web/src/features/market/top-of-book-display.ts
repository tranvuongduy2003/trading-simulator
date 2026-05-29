import type { OrderBookSnapshotResponse } from '@/features/market/api'
import { formatPrice } from '@/features/market/format-price'

export type TopOfBookTouch = {
  price: number
  quantity: number
} | null

export type TopOfBookSpread = {
  absolute: number
  percent: number
}

export type TopOfBookDisplay = {
  symbol: string
  bestBid: TopOfBookTouch
  bestAsk: TopOfBookTouch
  spread: TopOfBookSpread | null
  midPrice: number | null
}

export type BookLiquidityState = 'two-sided' | 'bid-only' | 'ask-only' | 'empty'

export const EMPTY_BOOK_MESSAGE = 'No market — waiting for liquidity'

const missingValueLabel = '—'
const noBidsLabel = 'No bids'
const noAsksLabel = 'No asks'

function round4(value: number): number {
  return Math.round(value * 10000) / 10000
}

function roundPercent3(value: number): number {
  return Math.round(value * 1000) / 1000
}

export function deriveSpreadAndMid(
  bestBid: TopOfBookTouch,
  bestAsk: TopOfBookTouch,
): { spread: TopOfBookSpread | null; midPrice: number | null } {
  if (!bestBid || !bestAsk) {
    return { spread: null, midPrice: null }
  }

  const spreadAbsolute = round4(bestAsk.price - bestBid.price)
  const midPrice = round4((bestBid.price + bestAsk.price) / 2)

  if (midPrice <= 0) {
    return { spread: null, midPrice: null }
  }

  const spreadPercent = roundPercent3((spreadAbsolute / midPrice) * 100)

  return {
    spread: { absolute: spreadAbsolute, percent: spreadPercent },
    midPrice,
  }
}

export function deriveTopOfBook(snapshot: OrderBookSnapshotResponse): TopOfBookDisplay {
  const bestBidLevel = snapshot.bids[0]
  const bestAskLevel = snapshot.asks[0]

  const bestBid = bestBidLevel
    ? { price: bestBidLevel.price, quantity: bestBidLevel.quantity }
    : null
  const bestAsk = bestAskLevel
    ? { price: bestAskLevel.price, quantity: bestAskLevel.quantity }
    : null
  const { spread, midPrice } = deriveSpreadAndMid(bestBid, bestAsk)

  return {
    symbol: snapshot.symbol,
    bestBid,
    bestAsk,
    spread,
    midPrice,
  }
}

export function deriveBookLiquidityState(display: TopOfBookDisplay): BookLiquidityState {
  const hasBid = display.bestBid !== null
  const hasAsk = display.bestAsk !== null

  if (hasBid && hasAsk) {
    return 'two-sided'
  }

  if (hasBid) {
    return 'bid-only'
  }

  if (hasAsk) {
    return 'ask-only'
  }

  return 'empty'
}

export function formatTopOfBookPrice(touch: TopOfBookTouch): string {
  if (!touch) {
    return missingValueLabel
  }

  return formatPrice(touch.price)
}

export function formatTopOfBookSidePrice(touch: TopOfBookTouch, side: 'bid' | 'ask'): string {
  if (touch) {
    return formatTopOfBookPrice(touch)
  }

  return side === 'bid' ? noBidsLabel : noAsksLabel
}

export function formatTopOfBookQuantity(touch: TopOfBookTouch): string | null {
  if (!touch || touch.quantity <= 0) {
    return null
  }

  return `${touch.quantity.toLocaleString('en-US')} shares`
}

export function formatSpreadAbsolute(spread: TopOfBookSpread | null): string {
  if (!spread) {
    return missingValueLabel
  }

  return formatPrice(spread.absolute)
}

export function formatSpreadPercent(spread: TopOfBookSpread | null): string | null {
  if (!spread) {
    return null
  }

  return `${spread.percent.toFixed(3)}%`
}

export function formatMidPrice(midPrice: number | null): string {
  if (midPrice === null) {
    return missingValueLabel
  }

  return formatPrice(midPrice)
}

export const MARKET_LOAD_ERROR_MESSAGE = 'Market data unavailable'
