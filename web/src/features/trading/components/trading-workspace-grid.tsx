import type { ReactNode } from 'react'

type TradingWorkspaceGridProps = {
  left: ReactNode
  center: ReactNode
  right: ReactNode
}

export function TradingWorkspaceGrid({ left, center, right }: TradingWorkspaceGridProps) {
  return (
    <div className="flex flex-col gap-4 xl:grid xl:grid-cols-[minmax(17rem,20rem)_minmax(0,1fr)_minmax(16rem,18rem)] xl:items-start xl:gap-4">
      <section className="flex min-w-0 flex-col gap-4" aria-label="Order book and top of book">
        {left}
      </section>
      <section className="min-w-0" aria-label="Price chart">
        {center}
      </section>
      <section className="min-w-0" aria-label="Order entry">
        {right}
      </section>
    </div>
  )
}
