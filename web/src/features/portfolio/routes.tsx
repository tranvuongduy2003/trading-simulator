import type { RouteObject } from 'react-router-dom'

import { PortfolioPage } from './pages/portfolio-page'

export const portfolioRoutes: RouteObject[] = [{ path: 'portfolio', element: <PortfolioPage /> }]
