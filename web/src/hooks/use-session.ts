import { useQuery } from '@tanstack/react-query'
import { useEffect } from 'react'

import { authApi } from '@/features/auth'
import { ApiError } from '@/types/api-problem'
import { useAuthStore } from '@/store/auth-store'

export function useSession() {
  const setSession = useAuthStore((state) => state.setSession)
  const clearSession = useAuthStore((state) => state.clearSession)
  const setStatus = useAuthStore((state) => state.setStatus)

  const query = useQuery({
    queryKey: ['auth', 'session'],
    queryFn: ({ signal }) => authApi.getSession(signal),
    retry: false,
    staleTime: 60_000,
  })

  useEffect(() => {
    if (query.isPending) {
      setStatus('unknown')
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

    const wallet = query.data as { userIdentifier?: string; displayName?: string }

    setSession({
      userIdentifier:
        typeof wallet.userIdentifier === 'string' ? wallet.userIdentifier : 'session-active',
      displayName: wallet.displayName ?? null,
    })
  }, [clearSession, query.data, query.error, query.isError, query.isPending, setSession, setStatus])

  return query
}
