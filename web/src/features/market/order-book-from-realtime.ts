import type { OrderBookLevelResponse, OrderBookSnapshotResponse } from '@/features/market/api'
import type { OrderBookLevel, OrderBookUpdatedMessage } from '@/types/realtime'

export function mapOrderBookUpdatedToSnapshot(
  message: OrderBookUpdatedMessage,
): OrderBookSnapshotResponse {
  return {
    symbol: message.symbol,
    bids: normalizeSideLevels(message.bids),
    asks: normalizeSideLevels(message.asks),
    updatedAt: message.updatedAt,
  }
}

function normalizeSideLevels(levels: OrderBookLevel[]): OrderBookLevelResponse[] {
  const lastIndexByPrice = new Map<number, number>()

  levels.forEach((level, index) => {
    if (level.quantity > 0) {
      lastIndexByPrice.set(level.price, index)
    }
  })

  return levels
    .map((level, index) => ({ level, index }))
    .filter(({ level, index }) => level.quantity > 0 && lastIndexByPrice.get(level.price) === index)
    .map(({ level }) => ({
      price: level.price,
      quantity: level.quantity,
      orderCount: level.orderCount ?? 0,
    }))
}
