import type { QueryClient } from '@tanstack/react-query'

import type { SimulationHubMessageInterceptor } from './types'

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
      // Prefix matches all user-scoped wallet keys: ['wallet', userId]
      queryClient.invalidateQueries({ queryKey: ['wallet'] })
      queryClient.invalidateQueries({ queryKey: ['portfolio'] })
    }

    if (message.name === 'onOrderFillNotified' || message.name === 'onOrderCancellationNotified') {
      queryClient.invalidateQueries({ queryKey: ['orders', 'open'] })
      queryClient.invalidateQueries({ queryKey: ['orders', 'history'] })
    }

    if (message.name === 'onOrderBookUpdated') {
      queryClient.invalidateQueries({ queryKey: ['market', 'orderbook'] })
    }
  }
}
