import { useQuery } from '@tanstack/react-query'

import { authApi } from '@/features/auth'
import { normalizeWallet } from '@/hooks/use-session'
import { useAuthStore } from '@/store/auth-store'

// Wallet reads are authoritative from GET /api/wallet (BR-08). staleTime 0 + refetch on
// focus corrects stale tabs (EC-07) without enabling global refetchOnWindowFocus in providers.
// When order mutations ship, also invalidate { queryKey: ['wallet'] } on success.
export function useWalletQuery() {
  const authStatus = useAuthStore((state) => state.status)
  const userId = useAuthStore((state) => state.userId)

  return useQuery({
    queryKey: ['wallet', userId],
    queryFn: ({ signal }) => authApi.getWallet(signal),
    staleTime: 0,
    refetchOnWindowFocus: true,
    enabled: authStatus === 'authenticated' && userId !== null,
    select: normalizeWallet,
  })
}
