import { Link } from 'react-router-dom'

import { paths } from '@/app/paths'

export function LoginPage() {
  return (
    <div className="flex flex-col gap-3">
      <h1 className="text-2xl font-semibold tracking-tight">Login</h1>
      <p className="text-muted-foreground text-sm">
        Auth form will live here. Session probe uses{' '}
        <code className="font-mono text-xs">GET /api/wallet</code> until dedicated session endpoints
        ship.
      </p>
      <p className="text-muted-foreground text-sm">
        No account?{' '}
        <Link to={paths.register} className="text-foreground underline-offset-4 hover:underline">
          Register
        </Link>
      </p>
    </div>
  )
}
