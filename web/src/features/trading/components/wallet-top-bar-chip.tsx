import { Skeleton } from '@/components/ui/skeleton'
import { useWalletQuery } from '@/features/trading/hooks/use-wallet-query'
import { formatUsd } from '@/lib/format'
import { useAuthStore } from '@/store/auth-store'

export function WalletTopBarChip() {
  const status = useAuthStore((state) => state.status)
  const walletQuery = useWalletQuery()

  if (status !== 'authenticated') {
    return null
  }

  if (walletQuery.isPending) {
    return (
      <div className="flex items-center" aria-busy="true" aria-label="Loading available cash">
        <Skeleton className="h-5 w-24" />
      </div>
    )
  }

  if (walletQuery.isError) {
    return (
      <span
        className="text-muted-foreground flex items-center gap-2 text-sm"
        aria-label="Available virtual cash unavailable"
      >
        <span className="hidden sm:inline">Cash</span>
        <span>Unavailable</span>
      </span>
    )
  }

  const wallet = walletQuery.data
  if (!wallet) {
    return null
  }

  const amount = formatUsd(wallet.availableBalance)

  return (
    <span
      className="border-border bg-muted/50 flex items-center gap-2 rounded-md border px-2.5 py-1 text-sm"
      aria-label={`Available virtual cash ${amount}`}
    >
      <span className="text-muted-foreground hidden sm:inline">Cash</span>
      <span className="font-medium tabular-nums">{amount}</span>
    </span>
  )
}
