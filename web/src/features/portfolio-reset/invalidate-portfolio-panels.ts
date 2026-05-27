import type { QueryClient } from '@tanstack/react-query'

import { portfolioPanelQueryKeys } from '@/lib/query-keys'

export function invalidatePortfolioPanels(queryClient: QueryClient, userId: string) {
  const panelKeys = [
    portfolioPanelQueryKeys.wallet(userId),
    portfolioPanelQueryKeys.portfolio(userId),
    portfolioPanelQueryKeys.ordersOpen(userId),
    portfolioPanelQueryKeys.ordersHistory(userId),
    portfolioPanelQueryKeys.trades(userId),
  ]

  for (const queryKey of panelKeys) {
    void queryClient.invalidateQueries({ queryKey })
  }
}
