import type { RouteObject } from 'react-router-dom'

import { TradingPage } from './pages/trading-page'

export const tradingRoutes: RouteObject[] = [{ path: 'trading', element: <TradingPage /> }]
