import { createBrowserRouter, Navigate } from 'react-router-dom'

import { paths } from '@/app/paths'
import { ProtectedRoute } from '@/app/routes/protected-route'
import { PublicRoute } from '@/app/routes/public-route'
import { authRoutes } from '@/features/auth'
import { ordersRoutes } from '@/features/orders'
import { portfolioRoutes } from '@/features/portfolio'
import { tradingRoutes } from '@/features/trading'
import { AppLayout } from '@/layouts/app-layout'
import { AuthLayout } from '@/layouts/auth-layout'

export const router = createBrowserRouter([
  {
    element: <PublicRoute />,
    children: [
      {
        element: <AuthLayout />,
        children: authRoutes,
      },
    ],
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { index: true, element: <Navigate to={paths.trading} replace /> },
          ...tradingRoutes,
          ...portfolioRoutes,
          ...ordersRoutes,
        ],
      },
    ],
  },
  { path: '*', element: <Navigate to={paths.trading} replace /> },
])
