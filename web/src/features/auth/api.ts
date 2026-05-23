import { apiClient } from '@/lib/api'

export type SessionResponse = {
  userIdentifier: string
  displayName: string
}

export type LoginRequest = {
  email: string
  password: string
}

export type RegisterRequest = {
  email: string
  password: string
  displayName: string
}

/** Probes cookie session; uses wallet route until a dedicated session endpoint exists. */
export function getSession(signal?: AbortSignal) {
  return apiClient.get<SessionResponse>('/api/wallet', { signal })
}

export function login(request: LoginRequest, signal?: AbortSignal) {
  return apiClient.post<void>('/api/auth/login', request, { signal })
}

export function register(request: RegisterRequest, signal?: AbortSignal) {
  return apiClient.post<void>('/api/users', request, { signal })
}

export function logout(signal?: AbortSignal) {
  return apiClient.post<void>('/api/auth/logout', undefined, { signal })
}
