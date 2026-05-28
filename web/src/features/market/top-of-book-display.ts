import type { OrderBookSnapshotResponse } from '@/features/market/api'
import { formatPrice } from '@/features/market/format-price'

export type TopOfBookTouch = {
  price: number
  quantity: number
} | null

export type TopOfBookMetrics = {
  spreadAbsolute: number | null
  spreadPercent: number | null
  midPrice: number | null
}

export type TopOfBookDisplay = {
  symbol: string
  bestBid: TopOfBookTouch
  bestAsk: TopOfBookTouch
  metrics: TopOfBookMetrics
}

const missingValueLabel = '—'

function roundTo4(value: number): number {
  return Math.round(value * 10000) / 10000
}

function roundPercent3(value: number): number {
  return Math.round(value * 1000) / 1000
}

export function computeTopOfBookMetrics(
  bestBid: TopOfBookTouch,
  bestAsk: TopOfBookTouch,
): TopOfBookMetrics {
  if (!bestBid || !bestAsk) {
    return { spreadAbsolute: null, spreadPercent: null, midPrice: null }
  }

  const midPrice = roundTo4((bestBid.price + bestAsk.price) / 2)
  const spreadAbsolute = roundTo4(bestAsk.price - bestBid.price)
  const spreadPercent = midPrice > 0 ? roundPercent3((spreadAbsolute / midPrice) * 100) : null

  return { spreadAbsolute, spreadPercent, midPrice }
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

  return {
    symbol: snapshot.symbol,
    bestBid,
    bestAsk,
    metrics: computeTopOfBookMetrics(bestBid, bestAsk),
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

export function formatSpreadAbsolute(spreadAbsolute: number | null): string {
  if (spreadAbsolute === null) {
    return missingValueLabel
  }

  return formatPrice(spreadAbsolute)
}

export function formatSpreadPercent(spreadPercent: number | null): string {
  if (spreadPercent === null) {
    return missingValueLabel
  }

  return `${spreadPercent.toFixed(3)}%`
}

export function formatMidPrice(midPrice: number | null): string {
  if (midPrice === null) {
    return missingValueLabel
  }

  return formatPrice(midPrice)
}

export const MARKET_LOAD_ERROR_MESSAGE = 'Market data unavailable'
