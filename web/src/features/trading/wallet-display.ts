// Wallet display formatters use the API triple (total, reserved, available) as returned —
// never derive available from total − reserved on the client (BR-03 / Story 2 integrity).

import { formatUsd } from '@/lib/format'

export const WALLET_LOAD_ERROR_MESSAGE =
  'Could not load account data. Try refreshing or sign in again.'

export const ZERO_RESERVED_HELPER = 'None tied up in open buy orders'

export function formatWalletBreakdownLine(totalBalance: number, reservedBalance: number): string {
  return `Total ${formatUsd(totalBalance)} · Reserved ${formatUsd(reservedBalance)}`
}

export function formatReservedHelper(reservedBalance: number): string {
  return `Open buy orders hold ${formatUsd(reservedBalance)}`
}

export function canDisplayWallet(
  wallet: { userId: string } | null | undefined,
  sessionUserId: string | null,
): wallet is { userId: string } {
  if (!wallet || !sessionUserId) {
    return false
  }

  return wallet.userId === sessionUserId
}
