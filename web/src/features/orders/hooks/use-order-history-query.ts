import { useQuery } from '@tanstack/react-query'

import { portfolioPanelQueryKeys } from '@/lib/query-keys'
import { useAuthStore } from '@/store/auth-store'

import * as ordersApi from '../api'

const defaultPageNumber = 1
const defaultPageSize = 25

export type UseOrderHistoryQueryOptions = {
  pageNumber?: number
  pageSize?: number
}

export function useOrderHistoryQuery(options: UseOrderHistoryQueryOptions = {}) {
  const authStatus = useAuthStore((state) => state.status)
  const userId = useAuthStore((state) => state.userId)
  const pageNumber = options.pageNumber ?? defaultPageNumber
  const pageSize = options.pageSize ?? defaultPageSize

  return useQuery({
    queryKey: [...portfolioPanelQueryKeys.ordersHistory(userId ?? ''), { pageNumber, pageSize }],
    queryFn: ({ signal }) => ordersApi.getOrderHistory({ pageNumber, pageSize }, signal),
    staleTime: 0,
    refetchOnWindowFocus: true,
    enabled: authStatus === 'authenticated' && userId !== null,
  })
}
