import { NavLink, Outlet } from 'react-router-dom'

import { paths } from '@/app/paths'
import { Separator } from '@/components/ui/separator'
import { cn } from '@/lib/utils'

const navigationItems = [
  { to: paths.trading, label: 'Trading' },
  { to: paths.portfolio, label: 'Portfolio' },
  { to: paths.orders, label: 'Orders' },
] as const

export function AppLayout() {
  return (
    <div className="flex min-h-screen flex-col">
      <header
        className="border-border bg-card/60 supports-[backdrop-filter]:bg-card/40 sticky top-0 z-40 border-b backdrop-blur"
        style={{ viewTransitionName: 'site-header' }}
      >
        <div className="mx-auto flex h-14 w-full max-w-7xl items-center gap-6 px-4">
          <p className="text-sm font-semibold tracking-tight">Trading Simulator</p>
          <nav className="flex items-center gap-1">
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
