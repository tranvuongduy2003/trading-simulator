import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useEffect } from 'react'
import { RouterProvider } from 'react-router-dom'

import { router } from '@/app/router'
import { Toaster } from '@/components/ui/sonner'
import { useSimulationHub } from '@/hooks/use-simulation-hub'
import { useAuthStore } from '@/store/auth-store'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
})

function RealtimeProvider() {
  useSimulationHub()
  return null
}

function UnauthorizedListener() {
  const clearSession = useAuthStore((state) => state.clearSession)

  useEffect(() => {
    const handleUnauthorized = () => {
      // Session bootstrap calls /api/wallet while logged out; 401 is expected.
      // Clearing the query cache here refetches that probe and traps public routes on the loader.
      if (useAuthStore.getState().status !== 'authenticated') {
        return
      }

      clearSession()
      queryClient.clear()
    }

    window.addEventListener('api:unauthorized', handleUnauthorized)
    return () => window.removeEventListener('api:unauthorized', handleUnauthorized)
  }, [clearSession])

  return null
}

export function AppProviders() {
  return (
    <QueryClientProvider client={queryClient}>
      <UnauthorizedListener />
      <RouterProvider router={router} />
      <RealtimeProvider />
      <Toaster richColors closeButton />
    </QueryClientProvider>
  )
}
