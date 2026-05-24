import type { UseFormSetError } from 'react-hook-form'

import { ApiError } from '@/types/api-problem'
import type { RegisterFormValues } from '@/types/auth'

const fieldErrorMessages: Record<string, Partial<Record<keyof RegisterFormValues, string>>> = {
  USERNAME_TAKEN: {
    username: 'That username is already in use.',
  },
  EMAIL_TAKEN: {
    email: 'An account with this email already exists.',
  },
}

export function applyRegisterApiError(
  error: unknown,
  setError: UseFormSetError<RegisterFormValues>,
): void {
  if (!(error instanceof ApiError)) {
    return
  }

  const code = error.problem.code
  if (code) {
    const mapped = fieldErrorMessages[code]
    if (mapped) {
      for (const [field, message] of Object.entries(mapped)) {
        setError(field as keyof RegisterFormValues, { message })
      }
      return
    }
  }

  const validationErrors = error.problem.errors
  if (!validationErrors) {
    return
  }

  for (const [field, messages] of Object.entries(validationErrors)) {
    setError(field as keyof RegisterFormValues, { message: messages.join(' ') })
  }
}
