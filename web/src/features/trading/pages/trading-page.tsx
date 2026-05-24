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
import { authApi } from '@/features/auth'
import { normalizeWallet } from '@/hooks/use-session'
import { formatUsd, toNumber } from '@/lib/format'

const AAPL_SYMBOL = 'AAPL'

export function TradingPage() {
  const walletQuery = useQuery({
    queryKey: ['wallet'],
    queryFn: ({ signal }) => authApi.getWallet(signal),
    staleTime: 30_000,
  })

  const portfolioQuery = useQuery({
    queryKey: ['portfolio'],
    queryFn: ({ signal }) => authApi.getPortfolio(signal),
    staleTime: 30_000,
  })

  const isLoading = walletQuery.isPending || portfolioQuery.isPending
  const hasError = walletQuery.isError || portfolioQuery.isError

  const wallet = walletQuery.data ? normalizeWallet(walletQuery.data) : null
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

      {isLoading ? (
        <div className="grid gap-4 md:grid-cols-2">
          <Skeleton className="h-32 w-full" />
          <Skeleton className="h-32 w-full" />
        </div>
      ) : null}

      {hasError ? (
        <p className="text-destructive text-sm" role="alert">
          Could not load account data. Try refreshing or sign in again.
        </p>
      ) : null}

      {wallet ? (
        <div className="grid gap-4 md:grid-cols-2">
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Virtual cash</CardTitle>
            </CardHeader>
            <CardContent className="flex flex-col gap-1">
              <p className="text-2xl font-semibold tabular-nums">
                {formatUsd(wallet.availableBalance)}
              </p>
              <p className="text-muted-foreground text-sm">Available to trade</p>
              <p className="text-muted-foreground text-xs tabular-nums">
                Total {formatUsd(wallet.totalBalance)} · Reserved{' '}
                {formatUsd(wallet.reservedBalance)}
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-base">Holdings</CardTitle>
            </CardHeader>
            <CardContent>
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
            </CardContent>
          </Card>
        </div>
      ) : null}
    </div>
  )
}
