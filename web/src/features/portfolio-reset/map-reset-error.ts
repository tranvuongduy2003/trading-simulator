import type { ApiProblem } from '@/types/api-problem'
import { ApiError } from '@/types/api-problem'

import { formatResetCooldownMessage } from './reset-eligibility'

export const resetUnauthorizedMessage =
  'Your session has expired. Sign in again to reset your portfolio.'

export const resetInProgressMessage =
  'A portfolio reset is already in progress. Please wait a moment and try again.'

export const resetCooldownActiveMessage = 'You can reset again after the 24-hour cooldown ends.'

export const resetInternalErrorMessage = 'Something went wrong. Please try again.'

export function readNextEligibleAtFromProblem(problem: ApiProblem): Date | null {
  const raw = problem.nextEligibleAt
  if (typeof raw !== 'string' || raw.length === 0) {
    return null
  }

  const parsed = new Date(raw)
  return Number.isNaN(parsed.getTime()) ? null : parsed
}

export function mapResetPortfolioError(error: unknown): string {
  if (!(error instanceof ApiError)) {
    return resetInternalErrorMessage
  }

  const code = error.problem.code

  if (code === 'RESET_COOLDOWN_ACTIVE') {
    const nextEligibleAt = readNextEligibleAtFromProblem(error.problem)
    if (nextEligibleAt) {
      return formatResetCooldownMessage(nextEligibleAt)
    }

    return resetCooldownActiveMessage
  }

  if (code === 'RESET_IN_PROGRESS' || error.status === 409) {
    return resetInProgressMessage
  }

  if (code === 'UNAUTHORIZED' || error.status === 401) {
    return resetUnauthorizedMessage
  }

  if (code === 'INTERNAL_ERROR' || error.status >= 500) {
    return resetInternalErrorMessage
  }

  return error.problem.detail ?? resetInternalErrorMessage
}
