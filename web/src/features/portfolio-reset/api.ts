import { apiClient } from '@/lib/api'

export type PortfolioResetWalletSnapshot = {
  totalBalance: number
  reservedBalance: number
  availableBalance: number
  currency: string
}

export type PortfolioResetResponse = {
  resetAt: string
  nextEligibleAt: string
  wallet: PortfolioResetWalletSnapshot
}

export function resetPortfolio() {
  return apiClient.post<PortfolioResetResponse>('/api/portfolio/reset', undefined, {
    credentials: 'include',
    suppressErrorToast: true,
  })
}
