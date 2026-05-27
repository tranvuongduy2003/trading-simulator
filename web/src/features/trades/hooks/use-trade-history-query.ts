import { useQuery } from '@tanstack/react-query'

import { portfolioPanelQueryKeys } from '@/lib/query-keys'
import { useAuthStore } from '@/store/auth-store'

import * as tradesApi from '../api'

const defaultPageNumber = 1
const defaultPageSize = 25

export type UseTradeHistoryQueryOptions = {
  pageNumber?: number
  pageSize?: number
}

export function useTradeHistoryQuery(options: UseTradeHistoryQueryOptions = {}) {
  const authStatus = useAuthStore((state) => state.status)
  const userId = useAuthStore((state) => state.userId)
  const pageNumber = options.pageNumber ?? defaultPageNumber
  const pageSize = options.pageSize ?? defaultPageSize

  return useQuery({
    queryKey: [...portfolioPanelQueryKeys.trades(userId ?? ''), { pageNumber, pageSize }],
    queryFn: ({ signal }) => tradesApi.getTradeHistory({ pageNumber, pageSize }, signal),
    staleTime: 0,
    refetchOnWindowFocus: true,
    enabled: authStatus === 'authenticated' && userId !== null,
  })
}
