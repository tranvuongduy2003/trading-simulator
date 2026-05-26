import { ApiError } from '@/types/api-problem'

export const resetUnauthorizedMessage =
  'Your session has expired. Sign in again to reset your portfolio.'

export const resetInProgressMessage =
  'A portfolio reset is already in progress. Please wait a moment and try again.'

export const resetInternalErrorMessage = 'Something went wrong. Please try again.'

export function mapResetPortfolioError(error: unknown): string {
  if (!(error instanceof ApiError)) {
    return resetInternalErrorMessage
  }

  const code = error.problem.code

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
