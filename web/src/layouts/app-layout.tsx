import { LogOutIcon, RotateCcwIcon } from 'lucide-react'
import { useState } from 'react'
import { NavLink, Outlet } from 'react-router-dom'

import { paths } from '@/app/paths'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { Separator } from '@/components/ui/separator'
import { Spinner } from '@/components/ui/spinner'
import { useLogout } from '@/features/auth/use-logout'
import { ResetPortfolioDialog } from '@/features/portfolio-reset/reset-portfolio-dialog'
import { useResetEligibility } from '@/features/portfolio-reset/reset-eligibility'
import { useResetPortfolio } from '@/features/portfolio-reset/use-reset-portfolio'
import { WalletTopBarChip } from '@/features/trading/components/wallet-top-bar-chip'
import { cn } from '@/lib/utils'
import { useAuthStore } from '@/store/auth-store'

const navigationItems = [
  { to: paths.trading, label: 'Trading' },
  { to: paths.portfolio, label: 'Portfolio' },
  { to: paths.orders, label: 'Orders' },
] as const

function UserMenu() {
  const [resetDialogOpen, setResetDialogOpen] = useState(false)
  const status = useAuthStore((state) => state.status)
  const userId = useAuthStore((state) => state.userId)
  const username = useAuthStore((state) => state.username)
  const { logout, isPending: isLogoutPending } = useLogout()
  const {
    resetPortfolio,
    isPending: isResetPending,
    errorMessage,
    clearError,
  } = useResetPortfolio()
  const { isResetAllowed, disabledHint } = useResetEligibility(userId)

  const isPending = isLogoutPending || isResetPending
  const isResetMenuDisabled = isPending || !isResetAllowed

  if (status !== 'authenticated') {
    return null
  }

  const displayName = username ?? 'Account'
  const initials =
    displayName
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('') || '?'

  return (
    <>
      <DropdownMenu>
        <DropdownMenuTrigger
          className={cn(
            'hover:bg-accent/50 focus-visible:ring-ring inline-flex h-9 items-center gap-2 rounded-md px-2 text-sm font-medium outline-none focus-visible:ring-2',
          )}
          aria-label="User menu"
          disabled={isPending}
        >
          <Avatar size="sm">
            <AvatarFallback>{initials}</AvatarFallback>
          </Avatar>
          <span className="max-w-[10rem] truncate">{displayName}</span>
          {isPending ? <Spinner className="size-4" /> : null}
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end" className="min-w-40">
          <DropdownMenuGroup>
            <DropdownMenuLabel className="font-normal">
              <span className="text-muted-foreground text-xs">Signed in as</span>
              <span className="block truncate font-medium">{displayName}</span>
            </DropdownMenuLabel>
          </DropdownMenuGroup>
          <DropdownMenuSeparator />
          <DropdownMenuItem
            disabled={isResetMenuDisabled}
            title={disabledHint ?? undefined}
            onSelect={() => {
              if (!isResetAllowed) {
                return
              }

              clearError()
              setResetDialogOpen(true)
            }}
          >
            <RotateCcwIcon />
            Reset portfolio
          </DropdownMenuItem>
          <DropdownMenuItem variant="destructive" disabled={isPending} onClick={() => logout()}>
            <LogOutIcon />
            Log out
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
      <ResetPortfolioDialog
        open={resetDialogOpen}
        onOpenChange={(open) => {
          if (!open) {
            clearError()
          }

          setResetDialogOpen(open)
        }}
        isPending={isResetPending}
        errorMessage={errorMessage}
        onConfirm={() => {
          resetPortfolio(undefined, {
            onSuccess: () => {
              setResetDialogOpen(false)
            },
          })
        }}
      />
    </>
  )
}

export function AppLayout() {
  return (
    <div className="flex min-h-screen flex-col">
      <header
        className="border-border bg-card/60 supports-[backdrop-filter]:bg-card/40 sticky top-0 z-40 border-b backdrop-blur"
        style={{ viewTransitionName: 'site-header' }}
      >
        <div className="mx-auto flex h-14 w-full max-w-7xl items-center gap-6 px-4">
          <p className="text-sm font-semibold tracking-tight">Trading Simulator</p>
          <nav className="flex flex-1 items-center gap-1">
            {navigationItems.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) =>
                  cn(
                    'rounded-md px-3 py-1.5 text-sm font-medium transition-colors',
                    isActive
                      ? 'bg-accent text-accent-foreground'
                      : 'text-muted-foreground hover:text-foreground hover:bg-accent/50',
                  )
                }
              >
                {item.label}
              </NavLink>
            ))}
          </nav>
          <div className="flex items-center gap-2">
            <WalletTopBarChip />
            <UserMenu />
          </div>
        </div>
      </header>

      <main className="mx-auto w-full max-w-7xl flex-1 px-4 py-6">
        <Outlet />
      </main>

      <footer className="border-border mt-auto border-t">
        <div className="text-muted-foreground mx-auto flex max-w-7xl items-center gap-3 px-4 py-3 text-xs">
          <span>AAPL · MVP shell</span>
          <Separator orientation="vertical" className="h-3" />
          <span>Real-time via SignalR</span>
        </div>
      </footer>
    </div>
  )
}
