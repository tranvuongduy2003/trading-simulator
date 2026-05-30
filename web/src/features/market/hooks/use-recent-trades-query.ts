import { useQuery } from '@tanstack/react-query'

import { getRecentTrades } from '@/features/market/api'
import { useAuthStore } from '@/store/auth-store'

export function useRecentTradesQuery() {
  const authStatus = useAuthStore((state) => state.status)

  return useQuery({
    queryKey: ['market', 'trades', 'AAPL'],
    queryFn: ({ signal }) => getRecentTrades('AAPL', 50, signal),
    staleTime: 0,
    refetchOnWindowFocus: true,
    enabled: authStatus === 'authenticated',
  })
}
