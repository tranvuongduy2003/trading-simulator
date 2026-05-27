import { useMutation } from '@tanstack/react-query'
import { useCallback, useState } from 'react'
import { toast } from 'sonner'

import { useAuthStore } from '@/store/auth-store'

import * as portfolioResetApi from './api'
import { mapResetPortfolioError } from './map-reset-error'
import { saveNextEligibleAt } from './reset-eligibility'

const resetSuccessTitle = 'Portfolio reset'
const resetSuccessDescription =
  "You're starting fresh with $100,000. Wallet balances update now; holdings and open orders will follow in a later release."

export function useResetPortfolio() {
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const clearError = useCallback(() => {
    setErrorMessage(null)
  }, [])

  const mutation = useMutation({
    mutationFn: () => portfolioResetApi.resetPortfolio(),
    onMutate: () => {
      setErrorMessage(null)
    },
    onSuccess: (response) => {
      const userId = useAuthStore.getState().userId
      if (userId) {
        saveNextEligibleAt(userId, response.nextEligibleAt)
      }

      toast.success(resetSuccessTitle, {
        description: resetSuccessDescription,
      })

      // Story 5: invalidate ['wallet'], ['portfolio'], ['orders'], ['trades'] here.
    },
    onError: (error) => {
      setErrorMessage(mapResetPortfolioError(error))
    },
  })

  return {
    resetPortfolio: mutation.mutate,
    isPending: mutation.isPending,
    errorMessage,
    clearError,
  }
}
