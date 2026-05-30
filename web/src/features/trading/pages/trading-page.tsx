import { OrderBookDepthPanel } from '@/features/market/components/order-book-depth-panel'
import { TopOfBookStrip } from '@/features/market/components/top-of-book-strip'
import { TradeTapePanel } from '@/features/market/components/trade-tape-panel'
import { useMarketConnectionStatus } from '@/features/market/hooks/use-market-connection-status'
import { useOrderBookQuery } from '@/features/market/hooks/use-order-book-query'
import { useRecentTradesQuery } from '@/features/market/hooks/use-recent-trades-query'
import { deriveTopOfBook } from '@/features/market/top-of-book-display'
import { PortfolioActivityTabs } from '@/features/trading/components/portfolio-activity-tabs'
import {
  ChartPlaceholder,
  OrderFormPlaceholder,
} from '@/features/trading/components/trading-workspace-placeholders'
import { TradingWorkspaceGrid } from '@/features/trading/components/trading-workspace-grid'
import { TradingWorkspaceHeader } from '@/features/trading/components/trading-workspace-header'
import { VirtualCashCard } from '@/features/trading/components/virtual-cash-card'
import { useWalletQuery } from '@/features/trading/hooks/use-wallet-query'
import { canDisplayWallet } from '@/features/trading/wallet-display'
import { ApiError } from '@/types/api-problem'
import { useAuthStore } from '@/store/auth-store'

export function TradingPage() {
  const sessionUserId = useAuthStore((state) => state.userId)
  const walletQuery = useWalletQuery()
  const orderBookQuery = useOrderBookQuery()
  const recentTradesQuery = useRecentTradesQuery()
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

  const refetchOrderBook = () => {
    void orderBookQuery.refetch()
  }

  const refetchRecentTrades = () => {
    void recentTradesQuery.refetch()
  }

  return (
    <div className="flex flex-col gap-6">
      <TradingWorkspaceHeader />

      <TradingWorkspaceGrid
        left={
          <>
            <TopOfBookStrip
              isPending={orderBookQuery.isPending}
              isError={orderBookQuery.isError}
              display={topOfBookDisplay}
              showReconnectingBadge={showReconnectingBadge}
              onRetry={refetchOrderBook}
            />
            <OrderBookDepthPanel
              isPending={orderBookQuery.isPending}
              isError={orderBookQuery.isError}
              snapshot={orderBookQuery.isSuccess ? orderBookQuery.data : null}
              showReconnectingBadge={showReconnectingBadge}
              onRetry={refetchOrderBook}
            />
            <TradeTapePanel
              isPending={recentTradesQuery.isPending}
              isError={recentTradesQuery.isError}
              snapshot={recentTradesQuery.isSuccess ? recentTradesQuery.data : null}
              onRetry={refetchRecentTrades}
            />
          </>
        }
        center={<ChartPlaceholder />}
        right={<OrderFormPlaceholder />}
      />

      <VirtualCashCard
        isPending={walletQuery.isPending}
        isError={showWalletError}
        wallet={displayWallet}
      />

      <PortfolioActivityTabs />
    </div>
  )
}
