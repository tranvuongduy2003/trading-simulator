import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import {
  formatReservedHelper,
  formatWalletBreakdownLine,
  WALLET_LOAD_ERROR_MESSAGE,
  ZERO_RESERVED_HELPER,
} from '@/features/trading/wallet-display'
import { normalizeWallet } from '@/hooks/use-session'
import { formatUsd } from '@/lib/format'

type NormalizedWallet = ReturnType<typeof normalizeWallet>

type VirtualCashCardProps = {
  isPending: boolean
  isError: boolean
  wallet: NormalizedWallet | null
}

export function VirtualCashCard({ isPending, isError, wallet }: VirtualCashCardProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Virtual cash</CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-1">
        {isPending ? (
          <div className="flex flex-col gap-2" aria-busy="true">
            <Skeleton className="h-8 w-40" />
            <Skeleton className="h-4 w-32" />
            <Skeleton className="h-3 w-48" />
          </div>
        ) : null}

        {isError ? (
          <p className="text-destructive text-sm" role="alert">
            {WALLET_LOAD_ERROR_MESSAGE}
          </p>
        ) : null}

        {wallet ? (
          <>
            {/* Available from API only — not total − reserved */}
            <p className="text-2xl font-semibold tabular-nums">
              {formatUsd(wallet.availableBalance)}
            </p>
            <p className="text-muted-foreground text-sm">Available to trade</p>
            <p className="text-muted-foreground text-xs tabular-nums">
              {formatWalletBreakdownLine(wallet.totalBalance, wallet.reservedBalance)}
            </p>
            <p className="text-muted-foreground text-xs">
              {wallet.reservedBalance > 0
                ? formatReservedHelper(wallet.reservedBalance)
                : ZERO_RESERVED_HELPER}
            </p>
          </>
        ) : null}
      </CardContent>
    </Card>
  )
}
