import { useQuery } from '@tanstack/react-query'

import { authApi } from '@/features/auth'
import { normalizeWallet } from '@/hooks/use-session'
import { useAuthStore } from '@/store/auth-store'

export function useWalletQuery() {
  const authStatus = useAuthStore((state) => state.status)
  const userId = useAuthStore((state) => state.userId)

  return useQuery({
    queryKey: ['wallet', userId],
    queryFn: ({ signal }) => authApi.getWallet(signal),
    staleTime: 30_000,
    enabled: authStatus === 'authenticated' && userId !== null,
    select: normalizeWallet,
  })
}
