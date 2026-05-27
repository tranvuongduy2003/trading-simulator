import { useQuery } from '@tanstack/react-query'

import { portfolioPanelQueryKeys } from '@/lib/query-keys'
import { useAuthStore } from '@/store/auth-store'

import * as ordersApi from '../api'

export function useOpenOrdersQuery() {
  const authStatus = useAuthStore((state) => state.status)
  const userId = useAuthStore((state) => state.userId)

  return useQuery({
    queryKey: portfolioPanelQueryKeys.ordersOpen(userId ?? ''),
    queryFn: ({ signal }) => ordersApi.getOpenOrders(signal),
    staleTime: 0,
    refetchOnWindowFocus: true,
    enabled: authStatus === 'authenticated' && userId !== null,
  })
}
