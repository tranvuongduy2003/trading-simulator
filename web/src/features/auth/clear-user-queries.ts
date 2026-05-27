import type { QueryClient } from '@tanstack/react-query'

export function clearUserScopedQueries(queryClient: QueryClient) {
  queryClient.removeQueries({ queryKey: ['wallet'] })
  queryClient.removeQueries({ queryKey: ['portfolio'] })
  queryClient.removeQueries({ queryKey: ['orders'] })
  queryClient.removeQueries({ queryKey: ['trades'] })
  queryClient.removeQueries({ queryKey: ['portfolio-reset', 'eligibility'] })
  queryClient.removeQueries({ queryKey: ['auth', 'session'] })
}
