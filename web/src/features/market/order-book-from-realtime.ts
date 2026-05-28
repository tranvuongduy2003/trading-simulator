import type { OrderBookSnapshotResponse } from '@/features/market/api'
import type { OrderBookUpdatedMessage } from '@/types/realtime'

export function mapOrderBookUpdatedToSnapshot(
  message: OrderBookUpdatedMessage,
): OrderBookSnapshotResponse {
  return {
    symbol: message.symbol,
    bids: message.bids.map((level) => ({
      price: level.price,
      quantity: level.quantity,
      orderCount: 0,
    })),
    asks: message.asks.map((level) => ({
      price: level.price,
      quantity: level.quantity,
      orderCount: 0,
    })),
    updatedAt: message.updatedAt,
  }
}
