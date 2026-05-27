import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useCallback, useState } from 'react'
import { toast } from 'sonner'

import type { WalletResponse } from '@/features/auth'
import { seedWalletQueryData } from '@/features/auth/prefetch-wallet'
import { ApiError } from '@/types/api-problem'
import { useAuthStore } from '@/store/auth-store'

import * as portfolioResetApi from './api'
import { invalidatePortfolioPanels } from './invalidate-portfolio-panels'
import { mapResetPortfolioError, readNextEligibleAtFromProblem } from './map-reset-error'
import { portfolioResetEligibilityQueryKey, saveNextEligibleAt } from './reset-eligibility'

const resetSuccessTitle = 'Portfolio reset'
const resetSuccessDescription =
  "You're starting fresh with $100,000. Wallet, holdings, and activity panels are updating."

function mapResetWalletToWalletResponse(
  snapshot: portfolioResetApi.PortfolioResetWalletSnapshot,
  userId: string,
  username: string,
): WalletResponse {
  return {
    userId,
    username,
    currency: snapshot.currency,
    totalBalance: snapshot.totalBalance,
    reservedBalance: snapshot.reservedBalance,
    availableBalance: snapshot.availableBalance,
  }
}

export function useResetPortfolio() {
  const queryClient = useQueryClient()
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
      const { userId, username } = useAuthStore.getState()

      if (userId) {
        saveNextEligibleAt(userId, response.nextEligibleAt)
        seedWalletQueryData(
          queryClient,
          userId,
          mapResetWalletToWalletResponse(response.wallet, userId, username ?? ''),
        )
        invalidatePortfolioPanels(queryClient, userId)
      }

      toast.success(resetSuccessTitle, {
        description: resetSuccessDescription,
      })

      void queryClient.invalidateQueries({ queryKey: portfolioResetEligibilityQueryKey })
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.code === 'RESET_COOLDOWN_ACTIVE') {
        const userId = useAuthStore.getState().userId
        const nextEligibleAt = readNextEligibleAtFromProblem(error.problem)
        if (userId && nextEligibleAt) {
          saveNextEligibleAt(userId, nextEligibleAt.toISOString())
          void queryClient.invalidateQueries({ queryKey: portfolioResetEligibilityQueryKey })
        }
      }

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
