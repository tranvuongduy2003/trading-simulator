import { useQuery } from '@tanstack/react-query'

import { authApi } from '@/features/auth'
import { normalizeWallet } from '@/hooks/use-session'

export function useWalletQuery() {
  return useQuery({
    queryKey: ['wallet'],
    queryFn: ({ signal }) => authApi.getWallet(signal),
    staleTime: 30_000,
    select: normalizeWallet,
  })
}
