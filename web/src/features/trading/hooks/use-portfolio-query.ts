import { useQuery } from '@tanstack/react-query'

import { authApi } from '@/features/auth'
import { useAuthStore } from '@/store/auth-store'

// Holdings reads are authoritative from GET /api/portfolio. staleTime 0 + refetch on
// focus corrects stale tabs after reset (EC-11) without global refetchOnWindowFocus.
export function usePortfolioQuery() {
  const authStatus = useAuthStore((state) => state.status)
  const userId = useAuthStore((state) => state.userId)

  return useQuery({
    queryKey: ['portfolio', userId],
    queryFn: ({ signal }) => authApi.getPortfolio(signal),
    staleTime: 0,
    refetchOnWindowFocus: true,
    enabled: authStatus === 'authenticated' && userId !== null,
  })
}
