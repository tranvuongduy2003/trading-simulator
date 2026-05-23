import { Navigate, Outlet } from 'react-router-dom'

import { paths } from '@/app/paths'
import { useAuthStore } from '@/store/auth-store'

export function PublicRoute() {
  const authStatus = useAuthStore((state) => state.status)

  if (authStatus === 'authenticated') {
    return <Navigate to={paths.trading} replace />
  }

  return <Outlet />
}
