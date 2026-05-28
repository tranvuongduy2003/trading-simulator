import type { QueryClient } from '@tanstack/react-query'

import { mapOrderBookUpdatedToSnapshot } from '@/features/market/order-book-from-realtime'
import type { OrderBookUpdatedMessage } from '@/types/realtime'

import type { SimulationHubMessageInterceptor } from './types'

const orderBookQueryKey = ['market', 'orderbook', 'AAPL'] as const

/** Logs inbound hub messages during local development. */
export const simulationHubDebugInterceptor: SimulationHubMessageInterceptor = (message) => {
  if (!import.meta.env.DEV) {
    return
  }

  console.debug('[signalr]', message.name, message.payload)
}

/** Example cache bridge — extend per message type as handlers land. */
export function createSimulationHubQueryBridge(
  queryClient: QueryClient,
): SimulationHubMessageInterceptor {
  return (message) => {
    if (message.name === 'onBalanceUpdated') {
      // Prefix matches all user-scoped panel keys (wallet, portfolio, trades).
      queryClient.invalidateQueries({ queryKey: ['wallet'] })
      queryClient.invalidateQueries({ queryKey: ['portfolio'] })
      queryClient.invalidateQueries({ queryKey: ['trades'] })
    }

    if (message.name === 'onOrderFillNotified' || message.name === 'onOrderCancellationNotified') {
      queryClient.invalidateQueries({ queryKey: ['orders', 'open'] })
      queryClient.invalidateQueries({ queryKey: ['orders', 'history'] })
      queryClient.invalidateQueries({ queryKey: ['trades'] })
    }

    if (message.name === 'onOrderBookUpdated') {
      const snapshot = mapOrderBookUpdatedToSnapshot(message.payload as OrderBookUpdatedMessage)
      queryClient.setQueryData(orderBookQueryKey, snapshot)
    }
  }
}
