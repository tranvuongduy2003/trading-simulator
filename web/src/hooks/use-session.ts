import { useQuery } from '@tanstack/react-query'
import { useEffect } from 'react'

import { authApi, type WalletResponse } from '@/features/auth'
import { toNumber } from '@/lib/format'
import { ApiError } from '@/types/api-problem'
import { useAuthStore } from '@/store/auth-store'

export function useSession() {
  const setSession = useAuthStore((state) => state.setSession)
  const clearSession = useAuthStore((state) => state.clearSession)
  const setStatus = useAuthStore((state) => state.setStatus)

  const query = useQuery({
    queryKey: ['auth', 'session'],
    queryFn: ({ signal }) => authApi.getWallet(signal),
    retry: false,
    staleTime: 60_000,
  })

  useEffect(() => {
    if (query.isPending) {
      if (useAuthStore.getState().status !== 'unauthenticated') {
        setStatus('unknown')
      }
      return
    }

    if (query.isError) {
      if (query.error instanceof ApiError && query.error.status === 401) {
        clearSession()
        return
      }

      setStatus('unauthenticated')
      return
    }

    const wallet = query.data
    setSession({
      userId: wallet.userId,
      username: wallet.username,
    })
  }, [clearSession, query.data, query.error, query.isError, query.isPending, setSession, setStatus])

  return query
}

export function normalizeWallet(wallet: WalletResponse) {
  return {
    ...wallet,
    totalBalance: toNumber(wallet.totalBalance),
    reservedBalance: toNumber(wallet.reservedBalance),
    availableBalance: toNumber(wallet.availableBalance),
  }
}
