import { Navigate, Outlet, useLocation } from 'react-router-dom'

import { useSession } from '@/hooks/use-session'
import { paths } from '@/app/paths'
import { useAuthStore } from '@/store/auth-store'
import { Skeleton } from '@/components/ui/skeleton'

// Full page refresh resets auth-store to unknown; useSession + useWalletQuery re-fetch
// GET /api/wallet from PostgreSQL (Story 4 — no persisted client wallet balances).
export function ProtectedRoute() {
  const location = useLocation()
  const authStatus = useAuthStore((state) => state.status)
  const sessionQuery = useSession()

  if (authStatus === 'unauthenticated') {
    return <Navigate to={paths.login} replace />
  }

  if (authStatus === 'unknown' || sessionQuery.isPending) {
    return (
      <div className="flex min-h-screen flex-col gap-3 p-8">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-4 w-full max-w-md" />
      </div>
    )
  }

  if (authStatus !== 'authenticated') {
    return <Navigate to={paths.login} replace state={{ from: location.pathname }} />
  }

  return <Outlet />
}
