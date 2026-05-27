import { useEffect, useState } from 'react'

const storageKey = 'portfolio-reset:nextEligibleAt'

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

export function formatResetUnavailableHint(
  userId: string | null,
  now: number = Date.now(),
): string | null {
  const nextEligibleAt = getNextEligibleAt(userId)
  if (!nextEligibleAt || now >= nextEligibleAt.getTime()) {
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

export function useResetEligibility(userId: string | null) {
  const [now, setNow] = useState(() => Date.now())

  useEffect(() => {
    const nextEligibleAt = getNextEligibleAt(userId)
    if (!nextEligibleAt) {
      return
    }

    const remainingMs = nextEligibleAt.getTime() - Date.now()
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
  }, [userId, now])

  const isResetAllowedNow = isResetAllowed(userId, now)
  const disabledHint = isResetAllowedNow ? null : formatResetUnavailableHint(userId, now)

  return {
    isResetAllowed: isResetAllowedNow,
    disabledHint,
  }
}
