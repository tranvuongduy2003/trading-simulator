import { Button } from '@/components/ui/button'
import { env } from '@/lib/env'

export function App() {
  return (
    <main className="mx-auto flex min-h-screen max-w-3xl flex-col justify-center gap-6 px-6">
      <p className="text-muted-foreground text-sm font-medium tracking-widest uppercase">
        Trading Simulator
      </p>
      <h1 className="text-4xl font-semibold tracking-tight">Design system ready</h1>
      <p className="text-muted-foreground">
        Tailwind v4, shadcn/ui (base-nova), and semantic tokens are configured. Run the Aspire
        AppHost to start the full stack.
      </p>
      <p className="border-border bg-card text-muted-foreground rounded-lg border px-4 py-3 font-mono text-sm">
        API target: {env.apiUrl}
      </p>
      <div className="flex flex-wrap gap-3">
        <Button>Primary</Button>
        <Button variant="outline">Outline</Button>
        <span className="border-bid/30 bg-bid/10 text-bid inline-flex items-center rounded-md border px-3 py-1.5 text-sm font-medium">
          Bid
        </span>
        <span className="border-ask/30 bg-ask/10 text-ask inline-flex items-center rounded-md border px-3 py-1.5 text-sm font-medium">
          Ask
        </span>
      </div>
    </main>
  )
}
