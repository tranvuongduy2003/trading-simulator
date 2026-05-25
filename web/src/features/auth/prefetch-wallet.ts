import type { QueryClient } from '@tanstack/react-query'

import { normalizeWallet } from '@/hooks/use-session'

import * as authApi from './api'

export async function prefetchWalletQuery(queryClient: QueryClient, userId: string) {
  const wallet = await queryClient.fetchQuery({
    queryKey: ['wallet', userId],
    queryFn: ({ signal }) => authApi.getWallet(signal),
  })
  const normalized = normalizeWallet(wallet)
  queryClient.setQueryData(['wallet', userId], normalized)
  return normalized
}

export function seedWalletQueryData(
  queryClient: QueryClient,
  userId: string,
  wallet: authApi.WalletResponse,
) {
  queryClient.setQueryData(['wallet', userId], normalizeWallet(wallet))
}
