import { apiClient, type components } from '@/lib/api'

export type TradeHistoryResponse = components['schemas']['TradeHistoryResponse']

export type TradeHistoryParams = {
  pageNumber?: number
  pageSize?: number
}

function buildPageQuery(params?: TradeHistoryParams): string {
  const search = new URLSearchParams()

  if (params?.pageNumber !== undefined) {
    search.set('pageNumber', String(params.pageNumber))
  }

  if (params?.pageSize !== undefined) {
    search.set('pageSize', String(params.pageSize))
  }

  const queryString = search.toString()
  return queryString ? `?${queryString}` : ''
}

export function getTradeHistory(params?: TradeHistoryParams, signal?: AbortSignal) {
  return apiClient.get<TradeHistoryResponse>(`/api/trades${buildPageQuery(params)}`, { signal })
}
