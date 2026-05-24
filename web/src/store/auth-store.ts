import { create } from 'zustand'

export type AuthStatus = 'unknown' | 'authenticated' | 'unauthenticated'

type AuthState = {
  status: AuthStatus
  userId: string | null
  username: string | null
  setSession: (session: { userId: string; username: string }) => void
  clearSession: () => void
  setStatus: (status: AuthStatus) => void
}

export const useAuthStore = create<AuthState>((set) => ({
  status: 'unknown',
  userId: null,
  username: null,
  setSession: (session) =>
    set({
      status: 'authenticated',
      userId: session.userId,
      username: session.username,
    }),
  clearSession: () =>
    set({
      status: 'unauthenticated',
      userId: null,
      username: null,
    }),
  setStatus: (status) => set({ status }),
}))
