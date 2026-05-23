import type { RouteObject } from 'react-router-dom'

import { OrdersPage } from './pages/orders-page'

export const ordersRoutes: RouteObject[] = [{ path: 'orders', element: <OrdersPage /> }]
