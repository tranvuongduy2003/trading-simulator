import { create } from 'zustand'

export type AuthStatus = 'unknown' | 'authenticated' | 'unauthenticated'

type AuthState = {
  status: AuthStatus
  userIdentifier: string | null
  displayName: string | null
  setSession: (session: { userIdentifier: string; displayName?: string | null }) => void
  clearSession: () => void
  setStatus: (status: AuthStatus) => void
}

export const useAuthStore = create<AuthState>((set) => ({
  status: 'unknown',
  userIdentifier: null,
  displayName: null,
  setSession: (session) =>
    set({
      status: 'authenticated',
      userIdentifier: session.userIdentifier,
      displayName: session.displayName ?? null,
    }),
  clearSession: () =>
    set({
      status: 'unauthenticated',
      userIdentifier: null,
      displayName: null,
    }),
  setStatus: (status) => set({ status }),
}))
