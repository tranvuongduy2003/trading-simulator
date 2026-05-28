import { TopOfBookStrip } from '@/features/market/components/top-of-book-strip'
import { useMarketConnectionStatus } from '@/features/market/hooks/use-market-connection-status'
import { useOrderBookQuery } from '@/features/market/hooks/use-order-book-query'
import { deriveTopOfBook } from '@/features/market/top-of-book-display'
import { PortfolioActivityTabs } from '@/features/trading/components/portfolio-activity-tabs'
import { VirtualCashCard } from '@/features/trading/components/virtual-cash-card'
import { useWalletQuery } from '@/features/trading/hooks/use-wallet-query'
import { canDisplayWallet } from '@/features/trading/wallet-display'
import { ApiError } from '@/types/api-problem'
import { useAuthStore } from '@/store/auth-store'

export function TradingPage() {
  const sessionUserId = useAuthStore((state) => state.userId)
  const walletQuery = useWalletQuery()
  const orderBookQuery = useOrderBookQuery()
  const showReconnectingBadge = useMarketConnectionStatus()

  const walletUnauthorized =
    walletQuery.isError && walletQuery.error instanceof ApiError && walletQuery.error.status === 401

  const displayWallet =
    walletQuery.isSuccess &&
    !walletUnauthorized &&
    canDisplayWallet(walletQuery.data, sessionUserId)
      ? walletQuery.data
      : null

  const showWalletError = walletQuery.isError || (walletQuery.isSuccess && !displayWallet)

  const topOfBookDisplay = orderBookQuery.isSuccess ? deriveTopOfBook(orderBookQuery.data) : null

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold tracking-tight">Trading</h1>
        <p className="text-muted-foreground text-sm">
          AAPL simulator — order book and chart modules will mount here.
        </p>
      </div>

      <div className="grid gap-4 md:max-w-2xl">
        <TopOfBookStrip
          isPending={orderBookQuery.isPending}
          isError={orderBookQuery.isError}
          display={topOfBookDisplay}
          showReconnectingBadge={showReconnectingBadge}
          onRetry={() => {
            void orderBookQuery.refetch()
          }}
        />

        <VirtualCashCard
          isPending={walletQuery.isPending}
          isError={showWalletError}
          wallet={displayWallet}
        />
      </div>

      <PortfolioActivityTabs />
    </div>
  )
}
