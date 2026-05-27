import { apiClient, type components } from '@/lib/api'

export type OpenOrderDto = components['schemas']['OpenOrderDto']
export type OrderHistoryResponse = components['schemas']['OrderHistoryResponse']

export type OrderHistoryParams = {
  pageNumber?: number
  pageSize?: number
}

function buildPageQuery(params?: OrderHistoryParams): string {
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

export function getOpenOrders(signal?: AbortSignal) {
  return apiClient.get<OpenOrderDto[]>('/api/orders/open', { signal })
}

export function getOrderHistory(params?: OrderHistoryParams, signal?: AbortSignal) {
  return apiClient.get<OrderHistoryResponse>(`/api/orders/history${buildPageQuery(params)}`, {
    signal,
  })
}
