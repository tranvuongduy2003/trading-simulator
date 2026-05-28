import { useQuery } from '@tanstack/react-query'

import { getOrderBook } from '@/features/market/api'
import { useAuthStore } from '@/store/auth-store'

export function useOrderBookQuery() {
  const authStatus = useAuthStore((state) => state.status)

  return useQuery({
    queryKey: ['market', 'orderbook', 'AAPL'],
    queryFn: ({ signal }) => getOrderBook('AAPL', signal),
    staleTime: 0,
    refetchOnWindowFocus: true,
    enabled: authStatus === 'authenticated',
  })
}
