import { useQuery } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import { useAuthStore } from '@/store/auth-store'

import * as portfolioResetApi from './api'

const storageKey = 'portfolio-reset:nextEligibleAt'

export const portfolioResetEligibilityQueryKey = ['portfolio-reset', 'eligibility'] as const

type StoredEligibility = {
  userId: string
  nextEligibleAt: string
}

export function saveNextEligibleAt(userId: string, nextEligibleAt: string): void {
  try {
    const payload: StoredEligibility = { userId, nextEligibleAt }
    sessionStorage.setItem(storageKey, JSON.stringify(payload))
  } catch {
    // sessionStorage may be unavailable in private browsing
  }
}

export function getNextEligibleAt(userId: string | null): Date | null {
  if (!userId) {
    return null
  }

  try {
    const raw = sessionStorage.getItem(storageKey)
    if (!raw) {
      return null
    }

    const parsed = JSON.parse(raw) as StoredEligibility
    if (parsed.userId !== userId) {
      return null
    }

    const nextEligibleAt = new Date(parsed.nextEligibleAt)
    return Number.isNaN(nextEligibleAt.getTime()) ? null : nextEligibleAt
  } catch {
    return null
  }
}

export function isResetAllowed(userId: string | null, now: number = Date.now()): boolean {
  const nextEligibleAt = getNextEligibleAt(userId)
  if (!nextEligibleAt) {
    return true
  }

  return now >= nextEligibleAt.getTime()
}

export function formatResetCooldownMessage(nextEligibleAt: Date, now: number = Date.now()): string {
  const hint = formatCooldownHint(nextEligibleAt, now)
  if (hint) {
    return hint.replace('Next reset available in', 'You can reset again in')
  }

  return 'You can reset again now.'
}

export function formatCooldownHint(nextEligibleAt: Date, now: number = Date.now()): string | null {
  if (now >= nextEligibleAt.getTime()) {
    return null
  }

  const diffMs = nextEligibleAt.getTime() - now
  const diffMinutes = Math.ceil(diffMs / 60_000)

  if (diffMinutes < 60) {
    return `Next reset available in ${diffMinutes} minute${diffMinutes === 1 ? '' : 's'}`
  }

  const diffHours = Math.ceil(diffMinutes / 60)
  if (diffHours < 48) {
    return `Next reset available in ${diffHours} hour${diffHours === 1 ? '' : 's'}`
  }

  const diffDays = Math.ceil(diffHours / 24)
  return `Next reset available in ${diffDays} day${diffDays === 1 ? '' : 's'}`
}

export function formatResetUnavailableHint(
  userId: string | null,
  now: number = Date.now(),
): string | null {
  const nextEligibleAt = getNextEligibleAt(userId)
  if (!nextEligibleAt) {
    return null
  }

  return formatCooldownHint(nextEligibleAt, now)
}

function resolveDisabledHint(
  isResetAllowedNow: boolean,
  userId: string | null,
  serverEligibility: portfolioResetApi.PortfolioResetEligibilityResponse | undefined,
  now: number,
): string | null {
  if (isResetAllowedNow) {
    return null
  }

  if (serverEligibility?.nextEligibleAt) {
    const nextEligibleAt = new Date(serverEligibility.nextEligibleAt)
    if (!Number.isNaN(nextEligibleAt.getTime())) {
      const hint = formatCooldownHint(nextEligibleAt, now)
      if (hint) {
        return hint
      }
    }
  }

  return formatResetUnavailableHint(userId, now)
}

export function useResetEligibility(userId: string | null) {
  const authStatus = useAuthStore((state) => state.status)
  const [now, setNow] = useState(() => Date.now())

  const eligibilityQuery = useQuery({
    queryKey: [...portfolioResetEligibilityQueryKey, userId],
    queryFn: ({ signal }) => portfolioResetApi.getResetEligibility(signal),
    enabled: authStatus === 'authenticated' && userId !== null,
    staleTime: 0,
    refetchOnWindowFocus: true,
  })

  useEffect(() => {
    const eligibility = eligibilityQuery.data
    if (!userId || !eligibility?.nextEligibleAt) {
      return
    }

    saveNextEligibleAt(userId, eligibility.nextEligibleAt)
  }, [userId, eligibilityQuery.data])

  const serverEligibility = eligibilityQuery.data
  const isResetAllowedNow =
    typeof serverEligibility?.isEligible === 'boolean'
      ? serverEligibility.isEligible
      : isResetAllowed(userId, now)

  const disabledHint = resolveDisabledHint(isResetAllowedNow, userId, serverEligibility, now)

  const timerAnchorMs =
    serverEligibility?.nextEligibleAt && !serverEligibility.isEligible
      ? (() => {
          const parsed = new Date(serverEligibility.nextEligibleAt)
          return Number.isNaN(parsed.getTime()) ? null : parsed.getTime()
        })()
      : (getNextEligibleAt(userId)?.getTime() ?? null)

  useEffect(() => {
    if (timerAnchorMs === null) {
      return
    }

    const remainingMs = timerAnchorMs - now
    if (remainingMs <= 0) {
      return
    }

    const intervalMs = Math.min(remainingMs, 60_000)
    const intervalId = window.setInterval(() => {
      setNow(Date.now())
    }, intervalMs)

    const timeoutId = window.setTimeout(() => {
      setNow(Date.now())
    }, remainingMs + 500)

    return () => {
      window.clearInterval(intervalId)
      window.clearTimeout(timeoutId)
    }
  }, [userId, timerAnchorMs, now])

  return {
    isResetAllowed: isResetAllowedNow,
    disabledHint,
  }
}
