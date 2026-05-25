import { useQuery } from '@tanstack/react-query'

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { VirtualCashCard } from '@/features/trading/components/virtual-cash-card'
import { useWalletQuery } from '@/features/trading/hooks/use-wallet-query'
import { authApi } from '@/features/auth'
import { toNumber } from '@/lib/format'

const AAPL_SYMBOL = 'AAPL'

const PORTFOLIO_LOAD_ERROR_MESSAGE = 'Could not load holdings. Try refreshing.'

export function TradingPage() {
  const walletQuery = useWalletQuery()

  const portfolioQuery = useQuery({
    queryKey: ['portfolio'],
    queryFn: ({ signal }) => authApi.getPortfolio(signal),
    staleTime: 30_000,
  })

  const aaplHolding = portfolioQuery.data?.holdings.find(
    (holding) => holding.symbol === AAPL_SYMBOL,
  )
  const aaplAvailable = aaplHolding ? toNumber(aaplHolding.availableQuantity) : 0

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold tracking-tight">Trading</h1>
        <p className="text-muted-foreground text-sm">
          AAPL simulator — order book and chart modules will mount here.
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <VirtualCashCard
          isPending={walletQuery.isPending}
          isError={walletQuery.isError}
          wallet={walletQuery.isSuccess ? walletQuery.data : null}
        />

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Holdings</CardTitle>
          </CardHeader>
          <CardContent>
            {portfolioQuery.isPending ? (
              <div className="flex flex-col gap-2" aria-busy="true">
                <Skeleton className="h-4 w-full" />
                <Skeleton className="h-4 w-full" />
              </div>
            ) : null}

            {portfolioQuery.isError ? (
              <p className="text-muted-foreground text-sm" role="alert">
                {PORTFOLIO_LOAD_ERROR_MESSAGE}
              </p>
            ) : null}

            {portfolioQuery.isSuccess ? (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Symbol</TableHead>
                    <TableHead className="text-right">Available</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  <TableRow>
                    <TableCell className="font-medium">{AAPL_SYMBOL}</TableCell>
                    <TableCell className="text-right tabular-nums">{aaplAvailable}</TableCell>
                  </TableRow>
                </TableBody>
              </Table>
            ) : null}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
