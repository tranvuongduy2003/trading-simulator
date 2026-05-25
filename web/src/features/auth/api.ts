import { apiClient } from '@/lib/api'

export type WalletSummary = {
  currency: string
  totalBalance: number
  reservedBalance: number
  availableBalance: number
}

export type UserRegistrationResponse = {
  userId: string
  username: string
  email: string
  createdAt: string
  wallet: WalletSummary
}

export type WalletResponse = {
  userId: string
  username: string
  currency: string
  totalBalance: number
  reservedBalance: number
  availableBalance: number
}

export type Holding = {
  symbol: string
  totalQuantity: number
  reservedQuantity: number
  availableQuantity: number
  averagePrice: number
}

export type PortfolioResponse = {
  portfolioId: string
  userId: string
  holdings: Holding[]
}

export type LoginRequest = {
  email: string
  password: string
}

export type LoginUserResponse = {
  userId: string
  username: string
  email: string
}

export type RegisterRequest = {
  username: string
  email: string
  password: string
}

export function getWallet(signal?: AbortSignal) {
  return apiClient.get<WalletResponse>('/api/wallet', { signal })
}

/** @deprecated Use getWallet — kept for session probe naming. */
export const getSession = getWallet

export function getPortfolio(signal?: AbortSignal) {
  return apiClient.get<PortfolioResponse>('/api/portfolio', { signal })
}

export function login(request: LoginRequest, signal?: AbortSignal) {
  return apiClient.post<LoginUserResponse>('/api/auth/login', request, {
    signal,
    suppressErrorToast: true,
  })
}

export function register(request: RegisterRequest, signal?: AbortSignal) {
  return apiClient.post<UserRegistrationResponse>('/api/users', request, {
    signal,
    suppressErrorToast: true,
  })
}

export function logout(signal?: AbortSignal) {
  return apiClient.post<void>('/api/auth/logout', undefined, {
    signal,
    suppressErrorToast: true,
  })
}
